using NewBalanceShop.Application.Services;
using Xunit;

namespace NewBalanceShop.Tests.Unit.Services;

public class ProductSizesTests
{
    [Fact]
    public void Parse_SplitsAndTrims()
    {
        Assert.Equal(new[] { "40", "41", "42" }, ProductSizes.Parse(" 40, 41 ,42 ,"));
        Assert.Empty(ProductSizes.Parse(null));
    }

    [Fact]
    public void Resolve_WithEmptyRequestAndSingleSize_ReturnsThatSize()
    {
        Assert.Equal("One size", ProductSizes.Resolve("Cap", "One size", null));
    }

    [Fact]
    public void Resolve_WithEmptyRequestAndSeveralSizes_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => ProductSizes.Resolve("Shoe", "40,41", ""));
    }

    [Fact]
    public void Resolve_IsCaseInsensitive_AndReturnsCanonicalSize()
    {
        Assert.Equal("XL", ProductSizes.Resolve("Tee", "S,M,L,XL", "xl"));
    }

    [Fact]
    public void Resolve_WithUnknownSize_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => ProductSizes.Resolve("Shoe", "40,41", "50"));
    }

    [Fact]
    public void Resolve_ForProductWithoutSizes_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, ProductSizes.Resolve("Thing", "", "anything"));
    }
}
