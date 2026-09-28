using NewBalanceShop.Application.DTO;

namespace NewBalanceShop.Application.Interfaces;

public interface IAuthService
{
    Task<CustomerDto> RegisterAsync(RegisterDto dto);
    Task<CustomerDto> LoginAsync(LoginDto dto);
    Task<CustomerDto?> GetByIdAsync(int customerId);
    Task<CustomerDto> GoogleLoginAsync(string idToken);
}
