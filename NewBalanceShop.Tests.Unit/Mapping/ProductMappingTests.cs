using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Mapping;
using NewBalanceShop.Domain.Entities;
using Xunit;

namespace NewBalanceShop.Tests.Unit.Mapping;

public class ProductMappingTests
{
    [Fact]
    public void ToDto_MapsAllFields()
    {
        var product = new Product
        {
            Id = 1,
            Name = "New Balance 574",
            Brand = "New Balance",
            CategoryId = 2,
            Size = "42",
            Color = "Сірий",
            Price = 3299m,
            Stock = 15,
            ImageUrl = "/img.jpg",
            Description = "desc"
        };

        var dto = product.ToDto();

        Assert.Equal(product.Id, dto.Id);
        Assert.Equal(product.Name, dto.Name);
        Assert.Equal(product.Brand, dto.Brand);
        Assert.Equal(product.CategoryId, dto.CategoryId);
        Assert.Equal(product.Size, dto.Size);
        Assert.Equal(product.Color, dto.Color);
        Assert.Equal(product.Price, dto.Price);
        Assert.Equal(product.Stock, dto.Stock);
        Assert.Equal(product.ImageUrl, dto.ImageUrl);
        Assert.Equal(product.Description, dto.Description);
    }

    [Fact]
    public void ToEntity_MapsAllFields()
    {
        var dto = new ProductDto
        {
            Id = 1,
            Name = "New Balance 574",
            Brand = "New Balance",
            CategoryId = 2,
            Size = "42",
            Color = "Сірий",
            Price = 3299m,
            Stock = 15,
            ImageUrl = "/img.jpg",
            Description = "desc"
        };

        var entity = dto.ToEntity();

        Assert.Equal(dto.Id, entity.Id);
        Assert.Equal(dto.Name, entity.Name);
        Assert.Equal(dto.Brand, entity.Brand);
        Assert.Equal(dto.CategoryId, entity.CategoryId);
        Assert.Equal(dto.Size, entity.Size);
        Assert.Equal(dto.Color, entity.Color);
        Assert.Equal(dto.Price, entity.Price);
        Assert.Equal(dto.Stock, entity.Stock);
        Assert.Equal(dto.ImageUrl, entity.ImageUrl);
        Assert.Equal(dto.Description, entity.Description);
    }

    [Fact]
    public void ToDto_MapsGenderProductTypeAndOldPrice()
    {
        var product = new Product { Gender = "women", ProductType = "Running", Price = 100m, OldPrice = 150m };

        var dto = product.ToDto();

        Assert.Equal("women", dto.Gender);
        Assert.Equal("Running", dto.ProductType);
        Assert.Equal(150m, dto.OldPrice);
    }

    [Fact]
    public void ToEntity_MapsGenderProductTypeAndOldPrice()
    {
        var dto = new ProductDto { Gender = "kids", ProductType = "Socks", Price = 100m, OldPrice = 150m };

        var entity = dto.ToEntity();

        Assert.Equal("kids", entity.Gender);
        Assert.Equal("Socks", entity.ProductType);
        Assert.Equal(150m, entity.OldPrice);
    }

    [Fact]
    public void NewProduct_DefaultsToUnisexWithoutTypeAndNotOnSale()
    {
        var dto = new Product().ToDto();

        Assert.Equal("unisex", dto.Gender);
        Assert.Equal(string.Empty, dto.ProductType);
        Assert.Null(dto.OldPrice);
    }
}
