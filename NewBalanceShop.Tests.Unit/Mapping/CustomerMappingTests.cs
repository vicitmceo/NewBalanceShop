using NewBalanceShop.Application.Mapping;
using NewBalanceShop.Domain.Entities;
using Xunit;

namespace NewBalanceShop.Tests.Unit.Mapping;

public class CustomerMappingTests
{
    [Fact]
    public void ToDto_MapsAllPublicFields()
    {
        var customer = new Customer
        {
            Id = 1,
            FullName = "Тест Тестенко",
            Email = "t@t.com",
            City = "Kyiv",
            Country = "UA",
            Phone = "+380000000000",
            PasswordHash = "secret-hash"
        };

        var dto = customer.ToDto();

        Assert.Equal(customer.Id, dto.Id);
        Assert.Equal(customer.FullName, dto.FullName);
        Assert.Equal(customer.Email, dto.Email);
        Assert.Equal(customer.City, dto.City);
        Assert.Equal(customer.Country, dto.Country);
        Assert.Equal(customer.Phone, dto.Phone);
    }

    [Fact]
    public void ToDto_NeverExposesPasswordHash()
    {
        var customer = new Customer { Id = 1, PasswordHash = "secret-hash" };

        var dto = customer.ToDto();

        var dtoProperties = typeof(NewBalanceShop.Application.DTO.CustomerDto).GetProperties().Select(p => p.Name);
        Assert.DoesNotContain("PasswordHash", dtoProperties);
    }
}
