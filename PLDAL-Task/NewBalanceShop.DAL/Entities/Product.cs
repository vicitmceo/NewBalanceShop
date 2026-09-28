using System.ComponentModel.DataAnnotations;

namespace NewBalanceShop.DAL.Entities;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Введіть назву товару")]
    [StringLength(150, MinimumLength = 2)]
    [Display(Name = "Назва")]
    public string Name { get; set; } = string.Empty;

    [StringLength(80)]
    [Display(Name = "Бренд")]
    public string Brand { get; set; } = "New Balance";

    [Required(ErrorMessage = "Вкажіть категорію")]
    [StringLength(60)]
    [Display(Name = "Категорія")]
    public string Category { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "Розмір")]
    public string Size { get; set; } = string.Empty;

    [StringLength(40)]
    [Display(Name = "Колір")]
    public string Color { get; set; } = string.Empty;

    [Range(0, 1000000, ErrorMessage = "Ціна має бути додатною")]
    [Display(Name = "Ціна, ₴")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Залишок на складі")]
    public int Stock { get; set; }

    [StringLength(1000)]
    [Display(Name = "Опис")]
    public string Description { get; set; } = string.Empty;
}
