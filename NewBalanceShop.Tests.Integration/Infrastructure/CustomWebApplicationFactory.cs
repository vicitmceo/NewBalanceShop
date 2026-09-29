using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            // Program.cs реєструє ShopDbContext з UseNpgsql через AddDbContext, який окрім
            // DbContextOptions<ShopDbContext> додає ще й internal IDbContextOptionsConfiguration<T> —
            // якщо прибрати лише перший дескриптор, конфігурація Npgsql всеодно виконається
            // поруч з InMemory і EF впаде на "тільки один провайдер БД". Тож зносимо все,
            // що стосується ShopDbContext, і реєструємо його заново з InMemory.
            services.RemoveAll<DbContextOptions<ShopDbContext>>();
            services.RemoveAll<ShopDbContext>();
            services.RemoveAll<IDbContextOptionsConfiguration<ShopDbContext>>();

            services.AddDbContext<ShopDbContext>(options => options.UseInMemoryDatabase(_dbName));
        });
    }
}
