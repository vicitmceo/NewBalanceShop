using NewBalanceShop.DAL.Interfaces;
using NewBalanceShop.DAL.Persistence;

namespace NewBalanceShop.DAL.Repositories;

// Unit of Work тримає один спільний DbContext для всіх репозиторіїв
// і відповідає за єдину точку збереження змін.
public class UnitOfWork : IUnitOfWork
{
    private readonly ShopDbContext _db;
    private IProductRepository? _products;

    public UnitOfWork(ShopDbContext db)
    {
        _db = db;
    }

    public IProductRepository Products => _products ??= new ProductRepository(_db);

    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();

    public void Dispose() => _db.Dispose();
}
