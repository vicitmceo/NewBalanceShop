using NewBalanceShop.Domain.Entities;
using NewBalanceShop.Application.DTO;

namespace NewBalanceShop.Application.Interfaces;

/// <summary>Сценарії оформлення та перегляду замовлень.</summary>
public interface IOrderService
{
    /// <summary>Створює замовлення: перевіряє покупця, товар і склад, рахує суму, зменшує залишок.</summary>
    /// <param name="dto">Покупець і позиції.</param>
    /// <returns>Створене замовлення.</returns>
    Task<OrderDto> CreateAsync(CreateOrderDto dto);
    /// <summary>Замовлення за ключем.</summary>
    /// <param name="id">Ключ замовлення.</param>
    /// <returns>Замовлення або <c>null</c>.</returns>
    Task<OrderDto?> GetByIdAsync(int id);
    /// <summary>Історія замовлень покупця.</summary>
    /// <param name="customerId">Ключ покупця.</param>
    Task<List<OrderDto>> GetByCustomerAsync(int customerId);

    /// <summary>Усі замовлення магазину (для адміністратора), від новіших до старіших.</summary>
    Task<List<OrderDto>> GetAllAsync();

    /// <summary>Змінює статус замовлення. <c>false</c>, якщо замовлення не знайдено.</summary>
    Task<bool> UpdateStatusAsync(int id, OrderStatus status);
}
