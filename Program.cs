using Microsoft.EntityFrameworkCore;
using NewBalanceShop.Application.Interfaces;
using NewBalanceShop.Application.Services;
using NewBalanceShop.Domain.Interfaces;
using NewBalanceShop.Infrastructure.Data;
using NewBalanceShop.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Render надає порт через змінну середовища PORT — слухаємо саме її,
// якщо вона задана (локально ж використовується стандартний launchSettings)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Render видає підключення до Postgres через змінну DATABASE_URL у форматі
// postgres://user:pass@host:port/db — Npgsql такий формат не розуміє напряму,
// тож конвертуємо його в keyword=value рядок підключення
var connectionString = builder.Configuration.GetConnectionString("ShopDb");
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
                        $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}

builder.Services.AddDbContext<ShopDbContext>(options => options.UseNpgsql(connectionString));

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

app.UseSwagger();
app.UseSwaggerUI();

app.UseSession();

app.MapControllers();

app.Run();
