using NewBalanceShop.Application.Mapping;
using NewBalanceShop.Domain.Entities;
using Xunit;

namespace NewBalanceShop.Tests.Unit.Mapping;

public class OrderMappingTests
{
    [Fact]
    public void ToDto_MapsAllFieldsIncludingItems()
    {
        var order = new Order
        {
            Id = 1,
            CustomerId = 5,
            CreatedAt = new DateTime(2026, 1, 1),
            Status = OrderStatus.Confirmed,
            TotalPrice = 500m,
            Items = new List<OrderItem>
            {
                new()
                {
                    ProductId = 1,
                    Product = new Product { Id = 1, Name = "Товар" },
                    Quantity = 2,
                    UnitPrice = 250m
                }
            }
        };

        var dto = order.ToDto();

        Assert.Equal(order.Id, dto.Id);
        Assert.Equal(order.CustomerId, dto.CustomerId);
        Assert.Equal(order.Status, dto.Status);
        Assert.Equal(order.TotalPrice, dto.TotalPrice);
        Assert.Single(dto.Items);
        Assert.Equal("Товар", dto.Items[0].ProductName);
        Assert.Equal(2, dto.Items[0].Quantity);
        Assert.Equal(250m, dto.Items[0].UnitPrice);
    }

    [Fact]
    public void ToDto_WithEmptyItems_ReturnsEmptyItemsList()
    {
        var order = new Order { Id = 1, CustomerId = 1 };

        var dto = order.ToDto();

        Assert.Empty(dto.Items);
    }

    [Fact]
    public void ToDto_WithMissingProductNavigation_UsesEmptyProductName()
    {
        var order = new Order
        {
            Id = 1,
            CustomerId = 1,
            Items = new List<OrderItem> { new() { ProductId = 1, Product = null, Quantity = 1, UnitPrice = 10m } }
        };

        var dto = order.ToDto();

        Assert.Equal(string.Empty, dto.Items[0].ProductName);
    }
}
