using Microsoft.EntityFrameworkCore;
using NewBalanceShop.DAL.Entities;
using NewBalanceShop.DAL.Interfaces;
using NewBalanceShop.DAL.Persistence;

namespace NewBalanceShop.DAL.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ShopDbContext db) : base(db)
    {
    }

    public async Task<List<Product>> GetByCategoryAsync(string category)
    {
        return await DbSet.AsNoTracking()
            .Where(p => p.Category == category)
            .OrderBy(p => p.Id)
            .ToListAsync();
    }
}
