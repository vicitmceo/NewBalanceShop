using NewBalanceShop.Application.DTO;

namespace NewBalanceShop.Application.Interfaces;

/// <summary>Реєстрація й автентифікація покупців.</summary>
public interface IAuthService
{
    /// <summary>Реєструє покупця й зберігає хеш пароля.</summary>
    /// <param name="dto">Дані реєстрації.</param>
    /// <returns>Профіль нового покупця.</returns>
    Task<CustomerDto> RegisterAsync(RegisterDto dto);
    /// <summary>Перевіряє email і пароль.</summary>
    /// <param name="dto">Облікові дані.</param>
    /// <returns>Профіль покупця.</returns>
    Task<CustomerDto> LoginAsync(LoginDto dto);
    /// <summary>Профіль за ключем.</summary>
    /// <param name="customerId">Ключ покупця.</param>
    /// <returns>Профіль або <c>null</c>.</returns>
    Task<CustomerDto?> GetByIdAsync(int customerId);
    /// <summary>Вхід через Google: перевіряє Firebase ID-токен, створює покупця за потреби.</summary>
    /// <param name="idToken">Firebase ID-токен.</param>
    /// <returns>Профіль покупця.</returns>
    Task<CustomerDto> GoogleLoginAsync(string idToken);

    /// <summary>Вхід через GitHub: знаходить або створює покупця за логіном GitHub.</summary>
    /// <param name="login">Логін GitHub.</param>
    /// <param name="name">Відображуване ім'я (необов'язково).</param>
    Task<CustomerDto> GithubLoginAsync(string login, string? name);
}
