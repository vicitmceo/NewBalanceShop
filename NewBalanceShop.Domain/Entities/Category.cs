namespace NewBalanceShop.Domain.Entities;

/// <summary>Категорія товарів (кросівки, одяг, аксесуари).</summary>
public class Category
{
    /// <summary>Первинний ключ.</summary>
    public int Id { get; set; }
    /// <summary>Назва категорії.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Товари цієї категорії.</summary>
    public List<Product> Products { get; set; } = new();
}
