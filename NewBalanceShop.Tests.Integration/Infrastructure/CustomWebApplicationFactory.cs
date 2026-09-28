using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NewBalanceShop.Infrastructure.Data;

namespace NewBalanceShop.Tests.Integration.Infrastructure;

// Піднімає реальний Program.cs (весь DI-контейнер, middleware, контролери) через
// TestServer, але підміняє Postgres на ізольовану EF InMemory-базу для кожного тесту,
// щоб інтеграційні тести не залежали від зовнішньої БД і не заважали одне одному.
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"NewBalanceShopTests_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ShopDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            services.AddDbContext<ShopDbContext>(options => options.UseInMemoryDatabase(_dbName));
        });
    }
}
