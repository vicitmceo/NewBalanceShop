namespace NewBalanceShop.Domain.Entities;

/// <summary>Покупець або адміністратор магазину.</summary>
public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    /// <summary>PBKDF2-хеш пароля у форматі «ітерації.сіль.хеш». Порожній для входу через Google.</summary>
    public string PasswordHash { get; set; } = string.Empty;
    /// <summary>Ідентифікатор користувача Firebase, якщо він входив через Google.</summary>
    public string? GoogleUid { get; set; }
    /// <summary>Ознака блокування: заблокований покупець не може увійти.</summary>
    public bool IsBlocked { get; set; }
    /// <summary>Ознака адміністратора: відкриває доступ до адмін-ендпоінтів.</summary>
    public bool IsAdmin { get; set; }

    /// <summary>Замовлення покупця.</summary>
    public List<Order> Orders { get; set; } = new();
}
