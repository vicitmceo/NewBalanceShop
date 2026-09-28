using Microsoft.EntityFrameworkCore;
using NewBalanceShop.DAL.Entities;

namespace NewBalanceShop.DAL.Persistence;

public class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "New Balance 574", Brand = "New Balance", Category = "Кросівки", Size = "42", Color = "Сірий", Price = 3299m, Stock = 15, Description = "Класичні кросівки New Balance 574." },
            new Product { Id = 2, Name = "New Balance 550", Brand = "New Balance", Category = "Кросівки", Size = "43", Color = "Білий", Price = 4199m, Stock = 10, Description = "Ретро-баскетбольні кросівки New Balance 550." },
            new Product { Id = 3, Name = "NB Essentials Hoodie", Brand = "New Balance", Category = "Одяг", Size = "L", Color = "Чорний", Price = 1899m, Stock = 20, Description = "Худі з логотипом New Balance." },
            new Product { Id = 4, Name = "NB Runner Shorts", Brand = "New Balance", Category = "Одяг", Size = "M", Color = "Синій", Price = 1299m, Stock = 25, Description = "Легкі шорти для бігу New Balance." },
            new Product { Id = 5, Name = "NB Sports Cap", Brand = "New Balance", Category = "Аксесуари", Size = "One size", Color = "Червоний", Price = 599m, Stock = 40, Description = "Спортивна кепка New Balance." }
        );
    }
}
