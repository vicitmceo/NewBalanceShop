using Microsoft.EntityFrameworkCore;
using NewBalanceShop.Domain.Interfaces;
using NewBalanceShop.Infrastructure.Data;

namespace NewBalanceShop.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ShopDbContext Db;

    public Repository(ShopDbContext db)
    {
        Db = db;
    }

    public async Task<List<T>> GetAllAsync() => await Db.Set<T>().AsNoTracking().ToListAsync();

    public async Task<T?> GetByIdAsync(int id) => await Db.Set<T>().FindAsync(id);

    public async Task AddAsync(T entity) => await Db.Set<T>().AddAsync(entity);

    public void Update(T entity) => Db.Set<T>().Update(entity);

    public void Remove(T entity) => Db.Set<T>().Remove(entity);

    public async Task SaveChangesAsync() => await Db.SaveChangesAsync();
}
