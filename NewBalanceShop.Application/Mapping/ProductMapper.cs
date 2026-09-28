using NewBalanceShop.Application.DTO;
using NewBalanceShop.Domain.Entities;

namespace NewBalanceShop.Application.Mapping;

public static class ProductMapper
{
    public static ProductDto ToDto(this Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Brand = p.Brand,
        CategoryId = p.CategoryId,
        Size = p.Size,
        Color = p.Color,
        Price = p.Price,
        Stock = p.Stock,
        ImageUrl = p.ImageUrl,
        Description = p.Description
    };

    public static Product ToEntity(this ProductDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Brand = dto.Brand,
        CategoryId = dto.CategoryId,
        Size = dto.Size,
        Color = dto.Color,
        Price = dto.Price,
        Stock = dto.Stock,
        ImageUrl = dto.ImageUrl,
        Description = dto.Description
    };
}
