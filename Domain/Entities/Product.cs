using System.ComponentModel.DataAnnotations;

namespace NewBalanceShop.Domain.Entities;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(80)]
    public string Brand { get; set; } = "New Balance";

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [StringLength(20)]
    public string Size { get; set; } = string.Empty;

    [StringLength(40)]
    public string Color { get; set; } = string.Empty;

    [Range(0, 1000000)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [StringLength(300)]
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
}
