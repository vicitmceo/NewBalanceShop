namespace NewBalanceShop.Application.DTO;

// зберігається в сесії (не в БД) — це кошик неавторизованого відвідувача
// до моменту, поки він не оформить замовлення
public class CartItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class AddToCartDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}
