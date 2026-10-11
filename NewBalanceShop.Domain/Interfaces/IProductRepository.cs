using NewBalanceShop.Domain.Entities;

namespace NewBalanceShop.Domain.Interfaces;

/// <summary>Репозиторій товарів.</summary>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>Товари однієї категорії.</summary>
    /// <param name="categoryId">Ключ категорії.</param>
    Task<List<Product>> GetByCategoryAsync(int categoryId);
}
