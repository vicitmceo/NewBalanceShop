using NewBalanceShop.Domain.Entities;

namespace NewBalanceShop.Domain.Interfaces;

/// <summary>Репозиторій покупців.</summary>
public interface ICustomerRepository : IRepository<Customer>
{
    /// <summary>Знаходить покупця за email.</summary>
    /// <param name="email">Адреса електронної пошти.</param>
    /// <returns>Покупець або <c>null</c>.</returns>
    Task<Customer?> GetByEmailAsync(string email);
    /// <summary>Чи є в покупця замовлення.</summary>
    /// <param name="customerId">Ключ покупця.</param>
    Task<bool> HasOrdersAsync(int customerId);
}
