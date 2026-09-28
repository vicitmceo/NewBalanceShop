namespace NewBalanceShop.DAL.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }

    Task<int> SaveChangesAsync();
}
