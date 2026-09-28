using NewBalanceShop.Application.DTO;
using NewBalanceShop.Domain.Entities;

namespace NewBalanceShop.Application.Mapping;

public static class CustomerMapper
{
    public static CustomerDto ToDto(this Customer customer) => new()
    {
        Id = customer.Id,
        FullName = customer.FullName,
        Email = customer.Email,
        City = customer.City,
        Country = customer.Country,
        Phone = customer.Phone
    };
}
