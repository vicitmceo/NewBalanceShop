using Microsoft.EntityFrameworkCore;
using NewBalanceShop.Domain.Entities;

namespace NewBalanceShop.Infrastructure.Data;

public class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalPrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Кросівки" },
            new Category { Id = 2, Name = "Одяг" },
            new Category { Id = 3, Name = "Аксесуари" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "New Balance 574", Brand = "New Balance", CategoryId = 1, Size = "42", Color = "Сірий", Price = 3299m, Stock = 15, ImageUrl = "/images/nb574.jpg", Description = "Класичні кросівки New Balance 574." },
            new Product { Id = 2, Name = "New Balance 550", Brand = "New Balance", CategoryId = 1, Size = "43", Color = "Білий", Price = 4199m, Stock = 10, ImageUrl = "/images/nb550.jpg", Description = "Ретро-баскетбольні кросівки New Balance 550." },
            new Product { Id = 3, Name = "NB Essentials Hoodie", Brand = "New Balance", CategoryId = 2, Size = "L", Color = "Чорний", Price = 1899m, Stock = 20, ImageUrl = "/images/nb-hoodie.jpg", Description = "Худі з логотипом New Balance." }
        );
    }
}
