using NewBalanceShop.Application.DTO;
using NewBalanceShop.Domain.Entities;

namespace NewBalanceShop.Application.Mapping;

/// <summary>Перетворення між <c>Product</c> і <c>ProductDto</c>.</summary>
public static class ProductMapper
{
    public static ProductDto ToDto(this Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Brand = p.Brand,
        Gender = p.Gender,
        ProductType = p.ProductType,
        CategoryId = p.CategoryId,
        Size = p.Size,
        Color = p.Color,
        Price = p.Price,
        OldPrice = p.OldPrice,
        Stock = p.Stock,
        ImageUrl = p.ImageUrl,
        ExtraImages = p.ExtraImages,
        Description = p.Description
    };

    public static Product ToEntity(this ProductDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Brand = dto.Brand,
        Gender = dto.Gender,
        ProductType = dto.ProductType,
        CategoryId = dto.CategoryId,
        Size = dto.Size,
        Color = dto.Color,
        Price = dto.Price,
        OldPrice = dto.OldPrice,
        Stock = dto.Stock,
        ImageUrl = dto.ImageUrl,
        ExtraImages = dto.ExtraImages,
        Description = dto.Description
    };
}
