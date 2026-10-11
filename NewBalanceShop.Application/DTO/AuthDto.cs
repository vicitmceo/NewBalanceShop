namespace NewBalanceShop.Application.DTO;

/// <summary>Дані для реєстрації нового покупця.</summary>
public class RegisterDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

/// <summary>Дані для входу за email і паролем.</summary>
public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>Firebase ID-токен, отриманий фронтендом після входу через Google.</summary>
public class GoogleLoginDto
{
    public string IdToken { get; set; } = string.Empty;
}

/// <summary>Профіль покупця у відповідях API (без хеша пароля).</summary>
public class CustomerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsBlocked { get; set; }
    public bool IsAdmin { get; set; }
}

// FR-10: покупець редагує особисті дані (ПІБ, місто, країна, телефон, e-mail);
// пароль і історія замовлень через цей ендпоінт не змінюються
/// <summary>Поля профілю, які покупець може змінити: ПІБ, email, місто, країна, телефон.</summary>
public class UpdateCustomerDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
