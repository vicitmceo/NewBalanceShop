using System.Linq.Expressions;

namespace NewBalanceShop.DAL.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    IQueryable<T> Query(Expression<Func<T, bool>>? filter = null);
}
