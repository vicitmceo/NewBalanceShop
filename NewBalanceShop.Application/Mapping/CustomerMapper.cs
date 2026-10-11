using NewBalanceShop.Application.DTO;
using NewBalanceShop.Domain.Entities;

namespace NewBalanceShop.Application.Mapping;

/// <summary>Перетворення <c>Customer</c> у <c>CustomerDto</c>: хеш пароля у DTO не потрапляє.</summary>
public static class CustomerMapper
{
    public static CustomerDto ToDto(this Customer customer) => new()
    {
        Id = customer.Id,
        FullName = customer.FullName,
        Email = customer.Email,
        City = customer.City,
        Country = customer.Country,
        Phone = customer.Phone,
        IsBlocked = customer.IsBlocked,
        IsAdmin = customer.IsAdmin
    };
}
