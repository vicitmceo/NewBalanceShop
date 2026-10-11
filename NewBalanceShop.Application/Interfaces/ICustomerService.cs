using NewBalanceShop.Application.DTO;

namespace NewBalanceShop.Application.Interfaces;

/// <summary>Кабінет покупця та адміністрування користувачів.</summary>
public interface ICustomerService
{
    /// <summary>Усі користувачі (для адміністратора).</summary>
    Task<List<CustomerDto>> GetAllAsync();
    /// <summary>Профіль за ключем.</summary>
    /// <param name="id">Ключ покупця.</param>
    /// <returns>Профіль або <c>null</c>.</returns>
    Task<CustomerDto?> GetByIdAsync(int id);
    /// <summary>Оновлює профіль; email має бути унікальним.</summary>
    /// <param name="id">Ключ покупця.</param>
    /// <param name="dto">Нові дані.</param>
    /// <returns>Оновлений профіль або <c>null</c>, якщо покупця немає.</returns>
    Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto dto);
    /// <summary>Блокує або розблоковує покупця.</summary>
    /// <param name="id">Ключ покупця.</param>
    /// <param name="blocked">Нове значення блокування.</param>
    /// <returns>Оновлений профіль або <c>null</c>.</returns>
    Task<CustomerDto?> SetBlockedAsync(int id, bool blocked);
    /// <summary>Видаляє покупця.</summary>
    /// <param name="id">Ключ покупця.</param>
    /// <returns><c>false</c>, якщо покупця немає.</returns>
    Task<bool> DeleteAsync(int id);
}
