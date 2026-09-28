using NewBalanceShop.Domain.Entities;

namespace NewBalanceShop.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email);
}
