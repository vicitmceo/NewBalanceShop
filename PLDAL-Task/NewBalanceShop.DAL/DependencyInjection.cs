using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NewBalanceShop.DAL.Interfaces;
using NewBalanceShop.DAL.Persistence;
using NewBalanceShop.DAL.Repositories;

namespace NewBalanceShop.DAL;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string? connection)
    {
        services.AddDbContext<ShopDbContext>(options => options.UseSqlite(connection));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
