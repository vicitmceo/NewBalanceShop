using Microsoft.EntityFrameworkCore;
using NewBalanceShop.Domain.Entities;

namespace NewBalanceShop.Infrastructure.Data;

/// <summary>Контекст EF Core (PostgreSQL): таблиці магазину, зв'язки й початкові дані (seed).</summary>
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

        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.Email)
            .IsUnique();

        // історія замовлень не повинна зникати разом із покупцем
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Кросівки" },
            new Category { Id = 2, Name = "Одяг" },
            new Category { Id = 3, Name = "Аксесуари" }
        );

        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = 1, FullName = "Гість", Email = "guest@newbalanceshop.local", PasswordHash = "seed-no-auth-yet" },
            // сід-адмін для перевірки FR-12/FR-13 (перегляд/блокування/видалення користувачів) —
            // у репозиторії лише PBKDF2-хеш (формат PasswordHasher), сам пароль зберігається поза репозиторієм
            new Customer { Id = 2, FullName = "Адміністратор", Email = "admin@newbalanceshop.com", PasswordHash = "100000.zCfnaQu3HCBj8/w8gPw5qA==.6s19ORGZadTHN1m0Mjl/POEaX90xYzcaQbk6lOKMUZc=", IsAdmin = true }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "New Balance 574", Brand = "New Balance", CategoryId = 1, Size = "42", Color = "Сірий", Price = 3299m, Stock = 15, ImageUrl = "/images/nb574.jpg", Description = "Класичні кросівки New Balance 574." },
            new Product { Id = 2, Name = "New Balance 550", Brand = "New Balance", CategoryId = 1, Size = "43", Color = "Білий", Price = 4199m, Stock = 10, ImageUrl = "/images/nb550.jpg", Description = "Ретро-баскетбольні кросівки New Balance 550." },
            new Product { Id = 3, Name = "NB Essentials Hoodie", Brand = "New Balance", CategoryId = 2, Size = "L", Color = "Чорний", Price = 1899m, Stock = 20, ImageUrl = "", Description = "Худі з логотипом New Balance." },
            new Product { Id = 4, Name = "New Balance 480", Brand = "New Balance", CategoryId = 1, Size = "40", Color = "Бежевий", Price = 3799m, Stock = 12, ImageUrl = "/images/nb480.jpg", Description = "Низькі кросівки в стилі баскетбольної класики 80-х: шкіра, масивна підошва." },
            new Product { Id = 5, Name = "New Balance 1906R", Brand = "New Balance", CategoryId = 1, Size = "44", Color = "Срібний", Price = 5599m, Stock = 8, ImageUrl = "/images/nb1906r.jpg", Description = "Технологічні кросівки з металізованими вставками й амортизацією ABZORB." },
            new Product { Id = 6, Name = "New Balance 2002R", Brand = "New Balance", CategoryId = 1, Size = "41", Color = "Сливовий", Price = 5999m, Stock = 6, ImageUrl = "/images/nb2002r.jpg", Description = "Замша та сітка, підошва N-ERGY: комфорт на цілий день." },
            new Product { Id = 7, Name = "New Balance Numeric 480", Brand = "New Balance", CategoryId = 1, Size = "45", Color = "Коричневий", Price = 3999m, Stock = 9, ImageUrl = "/images/nb-numeric480.jpg", Description = "Скейт-версія 480 із замшею й посиленою підошвою." },
            new Product { Id = 8, Name = "NB Essentials Cap", Brand = "New Balance", CategoryId = 3, Size = "One size", Color = "Сірий", Price = 799m, Stock = 40, ImageUrl = "", Description = "Бейсболка з вишитим логотипом, регульований ремінець." },
            new Product { Id = 9, Name = "NB Sport Backpack", Brand = "New Balance", CategoryId = 3, Size = "One size", Color = "Чорний", Price = 1599m, Stock = 18, ImageUrl = "", Description = "Рюкзак 25 л для тренувань і міста, відділення для ноутбука." },
            new Product { Id = 10, Name = "NB Classic Tee", Brand = "New Balance", CategoryId = 2, Size = "M", Color = "Білий", Price = 999m, Stock = 30, ImageUrl = "", Description = "Бавовняна футболка прямого крою з логотипом на грудях." }
        );
    }
}
