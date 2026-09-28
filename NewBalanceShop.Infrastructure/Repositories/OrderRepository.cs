using Microsoft.EntityFrameworkCore;
using NewBalanceShop.Domain.Entities;
using NewBalanceShop.Domain.Interfaces;
using NewBalanceShop.Infrastructure.Data;

namespace NewBalanceShop.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ShopDbContext db) : base(db)
    {
    }

    public async Task<List<Order>> GetByCustomerAsync(int customerId)
    {
        return await Db.Orders.AsNoTracking()
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdWithItemsAsync(int id)
    {
        return await Db.Orders.AsNoTracking()
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}
