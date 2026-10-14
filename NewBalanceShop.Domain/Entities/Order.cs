namespace NewBalanceShop.Domain.Entities;

/// <summary>Статус замовлення.</summary>
public enum OrderStatus
{
    New,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}

/// <summary>Замовлення покупця з переліком позицій.</summary>
public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.New;
    /// <summary>Загальна сума замовлення.</summary>
    public decimal TotalPrice { get; set; }

    /// <summary>Позиції замовлення.</summary>
    public List<OrderItem> Items { get; set; } = new();
}

/// <summary>Позиція замовлення: товар, кількість і ціна на момент покупки.</summary>
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    /// <summary>Ціна одиниці на момент оформлення (не змінюється при зміні ціни товару).</summary>
    public decimal UnitPrice { get; set; }
    /// <summary>Обраний розмір (порожній, якщо в товару один розмір).</summary>
    public string Size { get; set; } = string.Empty;
}
