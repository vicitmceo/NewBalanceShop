using NewBalanceShop.Application.DTO;

namespace NewBalanceShop.Application.Interfaces;

/// <summary>Сценарії роботи з товарами (каталог і керування).</summary>
public interface IProductService
{
    /// <summary>Усі товари.</summary>
    Task<List<ProductDto>> GetAllAsync();
    /// <summary>Товари категорії.</summary>
    /// <param name="categoryId">Ключ категорії.</param>
    Task<List<ProductDto>> GetByCategoryAsync(int categoryId);
    /// <summary>Товар за ключем.</summary>
    /// <param name="id">Ключ товару.</param>
    /// <returns>Товар або <c>null</c>.</returns>
    Task<ProductDto?> GetByIdAsync(int id);
    /// <summary>Створює товар.</summary>
    /// <param name="dto">Дані товару.</param>
    /// <returns>Створений товар із присвоєним ключем.</returns>
    Task<ProductDto> CreateAsync(ProductDto dto);
    /// <summary>Оновлює товар.</summary>
    /// <param name="id">Ключ товару.</param>
    /// <param name="dto">Нові значення.</param>
    /// <returns><c>false</c>, якщо товару немає.</returns>
    Task<bool> UpdateAsync(int id, ProductDto dto);
    /// <summary>Видаляє товар.</summary>
    /// <param name="id">Ключ товару.</param>
    /// <returns><c>false</c>, якщо товару немає.</returns>
    Task<bool> DeleteAsync(int id);
}
