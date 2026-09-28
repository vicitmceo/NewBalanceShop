using NewBalanceShop.Application.DTO;
using NewBalanceShop.Domain.Entities;

namespace NewBalanceShop.Application.Mapping;

public static class OrderMapper
{
    public static OrderDto ToDto(this Order o) => new()
    {
        Id = o.Id,
        CustomerId = o.CustomerId,
        CreatedAt = o.CreatedAt,
        Status = o.Status,
        TotalPrice = o.TotalPrice,
        Items = o.Items.Select(i => new OrderItemDto
        {
            ProductId = i.ProductId,
            ProductName = i.Product?.Name ?? string.Empty,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        }).ToList()
    };
}
