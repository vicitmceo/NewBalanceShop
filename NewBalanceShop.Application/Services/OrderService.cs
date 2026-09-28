using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;
using NewBalanceShop.Application.Mapping;
using NewBalanceShop.Domain.Entities;
using NewBalanceShop.Domain.Interfaces;

namespace NewBalanceShop.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        ICustomerRepository customerRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto)
    {
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
        if (customer is null)
            throw new InvalidOperationException($"Покупця з id={dto.CustomerId} не знайдено.");

        var order = new Order
        {
            CustomerId = dto.CustomerId,
            Status = OrderStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        decimal total = 0m;

        foreach (var itemDto in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
            if (product is null)
                throw new InvalidOperationException($"Товар з id={itemDto.ProductId} не знайдено.");

            if (product.Stock < itemDto.Quantity)
                throw new InvalidOperationException($"Недостатньо товару '{product.Name}' на складі (в наявності: {product.Stock}).");

            product.Stock -= itemDto.Quantity;
            _productRepository.Update(product);

            var unitPrice = product.Price;
            total += unitPrice * itemDto.Quantity;

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = unitPrice
            });
        }

        order.TotalPrice = total;

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        var created = await _orderRepository.GetByIdWithItemsAsync(order.Id);
        return created!.ToDto();
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(id);
        return order?.ToDto();
    }

    public async Task<List<OrderDto>> GetByCustomerAsync(int customerId)
    {
        var orders = await _orderRepository.GetByCustomerAsync(customerId);
        return orders.Select(o => o.ToDto()).ToList();
    }
}
