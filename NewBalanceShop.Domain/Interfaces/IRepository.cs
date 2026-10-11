namespace NewBalanceShop.Domain.Interfaces;

/// <summary>Базовий контракт репозиторія: доступ до даних без прив'язки до EF Core.</summary>
/// <typeparam name="T">Тип сутності.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>Повертає всі записи (без відстеження змін).</summary>
    Task<List<T>> GetAllAsync();
    /// <summary>Знаходить запис за ключем.</summary>
    /// <param name="id">Первинний ключ.</param>
    /// <returns>Сутність або <c>null</c>.</returns>
    Task<T?> GetByIdAsync(int id);
    /// <summary>Додає сутність (збереження після <see cref="SaveChangesAsync"/>).</summary>
    Task AddAsync(T entity);
    /// <summary>Позначає сутність як змінену.</summary>
    void Update(T entity);
    /// <summary>Позначає сутність як видалену.</summary>
    void Remove(T entity);
    /// <summary>Зберігає всі накопичені зміни в БД.</summary>
    Task SaveChangesAsync();
}
