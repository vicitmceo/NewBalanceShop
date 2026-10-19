using Microsoft.AspNetCore.Mvc;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;

namespace NewBalanceShop.Presentation.Controllers;

/// <summary>REST-ендпоінт категорій товарів (лише перегляд).</summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>Список усіх категорій.</summary>
    /// <returns>200 і категорії за зростанням ключа.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        return Ok(await _categoryService.GetAllAsync());
    }
}
