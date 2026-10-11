using Microsoft.EntityFrameworkCore;
using NewBalanceShop.Domain.Entities;
using NewBalanceShop.Domain.Interfaces;
using NewBalanceShop.Infrastructure.Data;

namespace NewBalanceShop.Infrastructure.Repositories;

/// <summary>Репозиторій товарів на EF Core.</summary>
public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ShopDbContext db) : base(db)
    {
    }

    public async Task<List<Product>> GetByCategoryAsync(int categoryId)
    {
        return await Db.Products.AsNoTracking()
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }
}
