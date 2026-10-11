using Microsoft.AspNetCore.Mvc;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;

namespace NewBalanceShop.Presentation.Controllers;

/// <summary>REST-ендпоінти каталогу товарів: перегляд і CRUD.</summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>Список товарів, опційно за категорією.</summary>
    /// <param name="categoryId">Ключ категорії (необов'язково).</param>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll([FromQuery] int? categoryId)
    {
        var products = categoryId.HasValue
            ? await _productService.GetByCategoryAsync(categoryId.Value)
            : await _productService.GetAllAsync();

        return Ok(products);
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
    /// <returns>201 і створений товар.</returns>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Оновлює товар.</summary>
    /// <param name="id">Ключ товару.</param>
    /// <param name="dto">Нові значення.</param>
    /// <returns>204 або 404.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _productService.UpdateAsync(id, dto);
        if (!updated) return NotFound();

        return NoContent();
    }

    /// <summary>Видаляє товар.</summary>
    /// <param name="id">Ключ товару.</param>
    /// <returns>204 або 404.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _productService.DeleteAsync(id);
        if (!deleted) return NotFound();

        return NoContent();
    }
}
