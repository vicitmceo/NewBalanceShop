using NewBalanceShop.DAL;
using NewBalanceShop.DAL.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// PL + DAL (без BLL) — контролери звертаються до IUnitOfWork напряму
builder.Services.AddDataAccess(builder.Configuration.GetConnectionString("ShopDb"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    db.Database.EnsureCreated();
}

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.Run();
