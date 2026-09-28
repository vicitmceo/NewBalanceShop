using Microsoft.EntityFrameworkCore;
using NewBalanceShop.Domain.Entities;
using NewBalanceShop.Domain.Interfaces;
using NewBalanceShop.Infrastructure.Data;

namespace NewBalanceShop.Infrastructure.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ShopDbContext db) : base(db)
    {
    }

    public async Task<Customer?> GetByEmailAsync(string email) =>
        await Db.Set<Customer>().FirstOrDefaultAsync(c => c.Email == email);
}
