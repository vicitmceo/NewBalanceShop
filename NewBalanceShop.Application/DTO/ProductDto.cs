using System.ComponentModel.DataAnnotations;

namespace NewBalanceShop.Application.DTO;

/// <summary>Товар у запитах і відповідях API.</summary>
public class ProductDto : IValidatableObject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    /// <summary>Для кого товар: <c>men</c>, <c>women</c>, <c>kids</c> або <c>unisex</c>.</summary>
    [Required, RegularExpression("^(men|women|kids|unisex)$", ErrorMessage = "Gender має бути men, women, kids або unisex.")]
    public string Gender { get; set; } = "unisex";
    /// <summary>Тип товару ("Running", "Lifestyle", "Jackets &amp; Vests", "Socks"); порожній рядок — без типу.</summary>
    [StringLength(60)]
    public string ProductType { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public decimal Price { get; set; }
    /// <summary>Стара ціна до знижки; <c>null</c> — товар не на розпродажі, інакше має бути більшою за <see cref="Price"/>.</summary>
    [Range(0, 1000000)]
    public decimal? OldPrice { get; set; }
    public int Stock { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string ExtraImages { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>Перевіряє, що стара ціна (якщо задана) більша за поточну.</summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (OldPrice.HasValue && OldPrice.Value <= Price)
            yield return new ValidationResult("OldPrice має бути більшою за Price.", new[] { nameof(OldPrice) });
    }
}
