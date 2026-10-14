using NewBalanceShop.Domain.Entities;

namespace NewBalanceShop.Domain.Interfaces;

/// <summary>Репозиторій замовлень.</summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>Замовлення покупця з позиціями, нові спочатку.</summary>
    /// <param name="customerId">Ключ покупця.</param>
    Task<List<Order>> GetByCustomerAsync(int customerId);
    /// <summary>Замовлення за ключем разом із позиціями та товарами.</summary>
    /// <param name="id">Ключ замовлення.</param>
    /// <returns>Замовлення або <c>null</c>.</returns>
    Task<Order?> GetByIdWithItemsAsync(int id);

    /// <summary>Усі замовлення з позиціями, від новіших до старіших.</summary>
    Task<List<Order>> GetAllWithItemsAsync();
}
