using NewBalanceShop.Domain.Entities;

namespace NewBalanceShop.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<List<Product>> GetByCategoryAsync(int categoryId);
}
