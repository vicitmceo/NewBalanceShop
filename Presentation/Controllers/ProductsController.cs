using Microsoft.AspNetCore.Mvc;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;
using NewBalanceShop.Presentation.Security;

namespace NewBalanceShop.Presentation.Controllers;

/// <summary>REST-ендпоінти каталогу товарів: перегляд для всіх, зміни лише для адміністратора.</summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>Список товарів із фільтрами: категорія, пошук, бренд, діапазон цін, розмір, сортування.</summary>
    /// <param name="categoryId">Ключ категорії (необов'язково).</param>
    /// <param name="q">Підрядок у назві, описі чи кольорі (без урахування регістру).</param>
    /// <param name="brand">Точна назва бренду.</param>
    /// <param name="minPrice">Мінімальна ціна.</param>
    /// <param name="maxPrice">Максимальна ціна.</param>
    /// <param name="size">Розмір (точний збіг).</param>
    /// <param name="sort"><c>price_asc</c>, <c>price_desc</c> або <c>name</c>.</param>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll(
        [FromQuery] int? categoryId, [FromQuery] string? q, [FromQuery] string? brand,
        [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] string? size,
        [FromQuery] string? sort)
    {
        var products = categoryId.HasValue
            ? await _productService.GetByCategoryAsync(categoryId.Value)
            : await _productService.GetAllAsync();

        IEnumerable<ProductDto> result = products;
        if (!string.IsNullOrWhiteSpace(q))
            result = result.Where(p =>
                p.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                p.Color.Contains(q, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(brand))
            result = result.Where(p => string.Equals(p.Brand, brand, StringComparison.OrdinalIgnoreCase));
        if (minPrice.HasValue) result = result.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue) result = result.Where(p => p.Price <= maxPrice.Value);
        if (!string.IsNullOrWhiteSpace(size))
            result = result.Where(p => NewBalanceShop.Application.Services.ProductSizes.Parse(p.Size)
                .Any(x => string.Equals(x, size.Trim(), StringComparison.OrdinalIgnoreCase)));

        result = sort switch
        {
            "price_asc" => result.OrderBy(p => p.Price),
            "price_desc" => result.OrderByDescending(p => p.Price),
            "name" => result.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase),
            _ => result.OrderBy(p => p.Id)
        };

        return Ok(result.ToList());
    }

    /// <summary>Один товар.</summary>
    /// <param name="id">Ключ товару.</param>
    /// <returns>200 або 404.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null) return NotFound();

        return Ok(product);
    }

    /// <summary>Створює товар.</summary>
    /// <param name="dto">Дані товару.</param>
    /// <returns>201 і створений товар; 401/403 без прав адміністратора.</returns>
    [HttpPost]
    [RequireAdmin]
    public async Task<ActionResult<ProductDto>> Create(ProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Оновлює товар.</summary>
    /// <param name="id">Ключ товару.</param>
    /// <param name="dto">Нові значення.</param>
    /// <returns>204, 404; 401/403 без прав адміністратора.</returns>
    [HttpPut("{id}")]
    [RequireAdmin]
    public async Task<IActionResult> Update(int id, ProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _productService.UpdateAsync(id, dto);
        if (!updated) return NotFound();

        return NoContent();
    }

    /// <summary>Видаляє товар.</summary>
    /// <param name="id">Ключ товару.</param>
    /// <returns>204, 404; 401/403 без прав адміністратора.</returns>
    [HttpDelete("{id}")]
    [RequireAdmin]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _productService.DeleteAsync(id);
        if (!deleted) return NotFound();

        return NoContent();
    }
}
