using Microsoft.EntityFrameworkCore;
using NewBalanceShop.Application.Interfaces;
using NewBalanceShop.Application.Services;
using NewBalanceShop.Domain.Interfaces;
using NewBalanceShop.Infrastructure.Data;
using NewBalanceShop.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<ShopDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ShopDb")));

// сесія зберігає "кошик" неавторизованого відвідувача (список товарів
// до оформлення замовлення) — це тимчасові дані конкретного браузера,
// їх не потрібно писати в БД, поки покупець не натисне "Оформити замовлення"
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    db.Database.Migrate();
}

app.UseSession();

app.MapControllers();

app.Run();
