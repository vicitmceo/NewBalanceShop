using NewBalanceShop.Application.DTO;

namespace NewBalanceShop.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(CreateOrderDto dto);
    Task<OrderDto?> GetByIdAsync(int id);
    Task<List<OrderDto>> GetByCustomerAsync(int customerId);
}
