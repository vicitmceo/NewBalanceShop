using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
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
builder.Services.AddHttpClient();

// фронтенд (React, Netlify/Vercel) — інший домен, тож потрібен CORS з підтримкою
// кук (кошик зберігається в сесії, а не в БД, див. CartController)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// Render видає підключення до Postgres через змінну DATABASE_URL у форматі
// postgres://user:pass@host:port/db — Npgsql такий формат не розуміє напряму,
// тож конвертуємо його в keyword=value рядок підключення
var connectionString = builder.Configuration.GetConnectionString("ShopDb");
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    var dbPort = uri.Port == -1 ? 5432 : uri.Port;
    connectionString = $"Host={uri.Host};Port={dbPort};Database={uri.AbsolutePath.TrimStart('/')};" +
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
    // кросс-доменний фронтенд (Netlify/Vercel) — кука сесії має бути SameSite=None+Secure;
    // виняток — інтеграційні тести (NewBalanceShop.Tests.Integration), де WebApplicationFactory
    // ганяє запити по звичайному http://localhost, і Secure-кука там просто не зберігається
    // клієнтом між запитами, через що сесія (кошик, логін) "губиться" між тестовими викликами
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = builder.Environment.IsEnvironment("Testing")
        ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always;
});

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// вхід через Google (Firebase Authentication): службовий обліковий запис Firebase
// кладеться в env var, а не в файл у репозиторії (Firebase Console > Project
// Settings > Service Accounts > Generate new private key > вміст JSON в FIREBASE_SERVICE_ACCOUNT_JSON)
var firebaseCredentialsJson = Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT_JSON");
if (!string.IsNullOrEmpty(firebaseCredentialsJson) && FirebaseApp.DefaultInstance is null)
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromJson(firebaseCredentialsJson)
    });
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    // інтеграційні тести підміняють ShopDbContext на EF InMemory (NewBalanceShop.Tests.Integration),
    // а InMemory-провайдер не підтримує реляційні міграції — тож там БД просто створюється напряму
    if (app.Environment.IsEnvironment("Testing"))
        db.Database.EnsureCreated();
    else
        db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");

app.UseSession();

app.MapControllers();

app.Run();

// WebApplicationFactory<Program> (NewBalanceShop.Tests.Integration) потребує публічного
// класу Program, а top-level statements за замовчуванням генерують internal
public partial class Program;
