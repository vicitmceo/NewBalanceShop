using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;

namespace NewBalanceShop.Presentation.Controllers;

// Кошик неавторизованого покупця зберігається в сесії, а не в БД:
// у магазині ще немає реєстрації/входу, тож привʼязати кошик до
// облікового запису неможливо, а тримати його в БД для "нікого" сенсу
// немає — сесія якраз для такого тимчасового стану браузера.
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private const string CartSessionKey = "cart";

    private readonly IProductService _productService;
    private readonly IOrderService _orderService;

    public CartController(IProductService productService, IOrderService orderService)
    {
        _productService = productService;
        _orderService = orderService;
    }

    // GET api/cart
    [HttpGet]
    public ActionResult<IEnumerable<CartItemDto>> GetCart()
    {
        return Ok(GetCartFromSession());
    }

    // POST api/cart — додати товар (або збільшити кількість, якщо вже є в кошику)
    [HttpPost]
    public async Task<ActionResult<IEnumerable<CartItemDto>>> AddToCart(AddToCartDto dto)
    {
        var product = await _productService.GetByIdAsync(dto.ProductId);
        if (product is null) return NotFound(new { error = "Товар не знайдено" });

        var cart = GetCartFromSession();
        var existing = cart.FirstOrDefault(i => i.ProductId == dto.ProductId);

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
                Quantity = dto.Quantity
            });
        }

        SaveCartToSession(cart);
        return Ok(cart);
    }

    // DELETE api/cart/{productId} — прибрати товар з кошика
    [HttpDelete("{productId}")]
    public ActionResult<IEnumerable<CartItemDto>> RemoveFromCart(int productId)
    {
        var cart = GetCartFromSession();
        cart.RemoveAll(i => i.ProductId == productId);
        SaveCartToSession(cart);
        return Ok(cart);
    }

    // POST api/cart/checkout — оформити замовлення з поточного кошика сесії
    [HttpPost("checkout")]
    public async Task<ActionResult<OrderDto>> Checkout([FromQuery] int customerId)
    {
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
