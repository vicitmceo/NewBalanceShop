using NewBalanceShop.DAL.Entities;

namespace NewBalanceShop.DAL.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<List<Product>> GetByCategoryAsync(string category);
}
