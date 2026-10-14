using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;
using NewBalanceShop.Domain.Entities;
using NewBalanceShop.Presentation.Security;

namespace NewBalanceShop.Presentation.Controllers;

// Кошик неавторизованого покупця зберігається в сесії, а не в БД:
// у магазині ще немає реєстрації/входу, тож привʼязати кошик до
// облікового запису неможливо, а тримати його в БД для "нікого" сенсу
// немає — сесія якраз для такого тимчасового стану браузера.
/// <summary>Кошик покупця: зберігається в сесії, а не в БД, до оформлення замовлення.</summary>
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private const string CartSessionKey = "cart";

    private readonly IProductService _productService;
    private readonly IOrderService _orderService;

    private readonly ICustomerService _customerService;

    public CartController(IProductService productService, IOrderService orderService, ICustomerService customerService)
    {
        _customerService = customerService;
        _productService = productService;
        _orderService = orderService;
    }

    // GET api/cart
    /// <summary>Поточний кошик.</summary>
    [HttpGet]
    public ActionResult<IEnumerable<CartItemDto>> GetCart()
    {
        return Ok(GetCartFromSession());
    }

    // POST api/cart — додати товар (або збільшити кількість, якщо вже є в кошику)
    /// <summary>Додає товар у кошик (збільшує кількість, якщо вже є).</summary>
    /// <param name="dto">Товар і кількість.</param>
    /// <returns>Оновлений кошик або 404.</returns>
    [HttpPost]
    public async Task<ActionResult<IEnumerable<CartItemDto>>> AddToCart(AddToCartDto dto)
    {
        var product = await _productService.GetByIdAsync(dto.ProductId);
        if (product is null) return NotFound(new { error = "Товар не знайдено" });

        if (dto.Quantity < 1) return BadRequest(new { error = "Кількість має бути більшою за нуль" });

        var cart = GetCartFromSession();
        var existing = cart.FirstOrDefault(i => i.ProductId == dto.ProductId);

        // кількість у кошику не може перевищувати залишок на складі
        var wanted = dto.Quantity + (existing?.Quantity ?? 0);
        if (wanted > product.Stock)
            return BadRequest(new { error = $"Недостатньо товару '{product.Name}' на складі (в наявності: {product.Stock})." });

        if (existing is not null)
        {
            existing.Quantity += dto.Quantity;
        }
        else
        {
            cart.Add(new CartItemDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = dto.Quantity,
                ImageUrl = product.ImageUrl
            });
        }

        SaveCartToSession(cart);
        return Ok(cart);
    }

    // PUT api/cart/{productId} — встановити кількість
    /// <summary>Встановлює кількість товару в кошику (не більше залишку на складі).</summary>
    /// <param name="productId">Ключ товару.</param>
    /// <param name="dto">Нова кількість.</param>
    /// <returns>Оновлений кошик; 404, якщо товару немає в кошику; 400 при перевищенні залишку.</returns>
    [HttpPut("{productId}")]
    public async Task<ActionResult<IEnumerable<CartItemDto>>> SetQuantity(int productId, SetCartQuantityDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var cart = GetCartFromSession();
        var item = cart.FirstOrDefault(i => i.ProductId == productId);
        if (item is null) return NotFound(new { error = "Товару немає в кошику" });

        var product = await _productService.GetByIdAsync(productId);
        if (product is null) return NotFound(new { error = "Товар не знайдено" });
        if (dto.Quantity > product.Stock)
            return BadRequest(new { error = $"Недостатньо товару '{product.Name}' на складі (в наявності: {product.Stock})." });

        item.Quantity = dto.Quantity;
        SaveCartToSession(cart);
        return Ok(cart);
    }

    // DELETE api/cart/{productId} — прибрати товар з кошика
    /// <summary>Прибирає товар з кошика.</summary>
    /// <param name="productId">Ключ товару.</param>
    /// <returns>Оновлений кошик.</returns>
    [HttpDelete("{productId}")]
    public ActionResult<IEnumerable<CartItemDto>> RemoveFromCart(int productId)
    {
        var cart = GetCartFromSession();
        cart.RemoveAll(i => i.ProductId == productId);
        SaveCartToSession(cart);
        return Ok(cart);
    }

    // POST api/cart/checkout — оформити замовлення з поточного кошика сесії
    /// <summary>Оформлює замовлення з кошика й очищає його.</summary>
    /// <returns>Створене замовлення або 400. Покупець береться із сесії, гість оформлює на службового «Гостя».</returns>
    [HttpPost("checkout")]
    public async Task<ActionResult<OrderDto>> Checkout()
    {
        // ідентифікатор покупця ніколи не приймаємо від клієнта (захист від BOLA/IDOR)
        var me = await SessionAuth.GetCurrentAsync(HttpContext, _customerService);
        var customerId = me?.Id ?? Customer.GuestId;

        var cart = GetCartFromSession();
        if (cart.Count == 0) return BadRequest(new { error = "Кошик порожній" });

        var createDto = new CreateOrderDto
        {
            CustomerId = customerId,
            Items = cart.Select(i => new CreateOrderItemDto { ProductId = i.ProductId, Quantity = i.Quantity }).ToList()
        };

        try
        {
            var order = await _orderService.CreateAsync(createDto);
            HttpContext.Session.Remove(CartSessionKey); // кошик очищається після успішного замовлення
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private List<CartItemDto> GetCartFromSession()
    {
        var json = HttpContext.Session.GetString(CartSessionKey);
        return string.IsNullOrEmpty(json)
            ? new List<CartItemDto>()
            : JsonSerializer.Deserialize<List<CartItemDto>>(json) ?? new List<CartItemDto>();
    }

    private void SaveCartToSession(List<CartItemDto> cart)
    {
        HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
    }
}
