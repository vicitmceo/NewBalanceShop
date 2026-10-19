using System.ComponentModel.DataAnnotations;
using NewBalanceShop.Application.DTO;
using Xunit;

namespace NewBalanceShop.Tests.Unit.DTO;

public class ProductDtoValidationTests
{
    private static List<ValidationResult> Validate(ProductDto dto)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void DefaultProduct_IsValid()
    {
        Assert.Empty(Validate(new ProductDto { Price = 100m }));
    }

    [Theory]
    [InlineData("men")]
    [InlineData("women")]
    [InlineData("kids")]
    [InlineData("unisex")]
    public void KnownGender_IsValid(string gender)
    {
        Assert.Empty(Validate(new ProductDto { Gender = gender }));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Men")]
    [InlineData("alien")]
    public void UnknownGender_IsRejected(string gender)
    {
        var errors = Validate(new ProductDto { Gender = gender });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(ProductDto.Gender)));
    }

    [Fact]
    public void TooLongProductType_IsRejected()
    {
        var errors = Validate(new ProductDto { ProductType = new string('x', 61) });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(ProductDto.ProductType)));
    }

    [Fact]
    public void OldPriceGreaterThanPrice_IsValid()
    {
        Assert.Empty(Validate(new ProductDto { Price = 100m, OldPrice = 101m }));
    }

    [Theory]
    [InlineData(100, 100)]
    [InlineData(100, 50)]
    public void OldPriceNotGreaterThanPrice_IsRejected(int price, int oldPrice)
    {
        var errors = Validate(new ProductDto { Price = price, OldPrice = oldPrice });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(ProductDto.OldPrice)));
    }

    [Fact]
    public void NegativeOldPrice_IsRejected()
    {
        var errors = Validate(new ProductDto { Price = 0m, OldPrice = -1m });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(ProductDto.OldPrice)));
    }
}
