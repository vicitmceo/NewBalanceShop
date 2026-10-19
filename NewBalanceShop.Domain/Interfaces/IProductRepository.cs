using NewBalanceShop.Domain.Entities;

namespace NewBalanceShop.Domain.Interfaces;

/// <summary>Репозиторій товарів.</summary>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>Товари однієї категорії.</summary>
    /// <param name="categoryId">Ключ категорії.</param>
    Task<List<Product>> GetByCategoryAsync(int categoryId);
    /// <summary>Товари за набором ключів одним запитом (з відстеженням змін, для оновлення залишків).</summary>
    /// <param name="ids">Ключі товарів; дублікати ігноруються.</param>
    Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids);
}
