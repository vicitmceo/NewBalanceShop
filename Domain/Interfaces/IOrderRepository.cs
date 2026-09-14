using NewBalanceShop.Domain.Entities;

namespace NewBalanceShop.Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<List<Order>> GetByCustomerAsync(int customerId);
}
