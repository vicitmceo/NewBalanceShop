using System.ComponentModel.DataAnnotations;

namespace NewBalanceShop.Domain.Entities;

/// <summary>Товар магазину: взуття, одяг або аксесуар New Balance.</summary>
public class Product
{
    /// <summary>Первинний ключ.</summary>
    public int Id { get; set; }

    /// <summary>Назва моделі.</summary>
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Бренд (за замовчуванням New Balance).</summary>
    [StringLength(80)]
    public string Brand { get; set; } = "New Balance";

    /// <summary>Зовнішній ключ на <see cref="Category"/>.</summary>
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    /// <summary>Доступні розміри через кому (наприклад, "40,41,42" або "S,M,L"; для аксесуарів "One size").</summary>
    [StringLength(120)]
    public string Size { get; set; } = string.Empty;

    /// <summary>Колір.</summary>
    [StringLength(40)]
    public string Color { get; set; } = string.Empty;

    /// <summary>Ціна в гривнях, від 0 до 1 000 000.</summary>
    [Range(0, 1000000)]
    public decimal Price { get; set; }

    /// <summary>Кількість на складі; зменшується при оформленні замовлення.</summary>
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    /// <summary>Посилання на зображення товару.</summary>
    [StringLength(300)]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Додаткові зображення галереї: посилання через кому.</summary>
    [StringLength(1000)]
    public string ExtraImages { get; set; } = string.Empty;

    /// <summary>Опис товару.</summary>
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
}
