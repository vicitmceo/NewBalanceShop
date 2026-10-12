using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;
using NewBalanceShop.Application.Mapping;
using NewBalanceShop.Domain.Entities;
using NewBalanceShop.Domain.Interfaces;

namespace NewBalanceShop.Application.Services;

/// <summary>Реалізація <see cref="IAuthService"/>: реєстрація, вхід за паролем і через Google.</summary>
public class AuthService : IAuthService
{
    private readonly ICustomerRepository _customerRepository;

    private readonly IGoogleTokenVerifier _googleVerifier;

    public AuthService(ICustomerRepository customerRepository, IGoogleTokenVerifier googleVerifier)
    {
        _customerRepository = customerRepository;
        _googleVerifier = googleVerifier;
    }

    public async Task<CustomerDto> RegisterAsync(RegisterDto dto)
    {
        var existing = await _customerRepository.GetByEmailAsync(dto.Email);
        if (existing is not null)
            throw new InvalidOperationException($"Покупець з email '{dto.Email}' вже зареєстрований.");

        var customer = new Customer
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            City = dto.City,
            Country = dto.Country,
            Phone = dto.Phone
        };

        await _customerRepository.AddAsync(customer);
        await _customerRepository.SaveChangesAsync();

        return customer.ToDto();
    }

    public async Task<CustomerDto> LoginAsync(LoginDto dto)
    {
        var customer = await _customerRepository.GetByEmailAsync(dto.Email);
        if (customer is null || !PasswordHasher.Verify(dto.Password, customer.PasswordHash))
            throw new InvalidOperationException("Невірний email або пароль.");

        if (customer.IsBlocked)
            throw new InvalidOperationException("Обліковий запис заблоковано.");

        return customer.ToDto();
    }

    public async Task<CustomerDto?> GetByIdAsync(int customerId)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        return customer?.ToDto();
    }

    // ідентифікуємо покупця по ID-токену, виданому Firebase після входу через Google
    // на фронтенді (signInWithPopup + GoogleAuthProvider); FirebaseAuth сам звіряє
    // підпис токена з публічними ключами Google, тож паролі тут не потрібні
    public async Task<CustomerDto> GoogleLoginAsync(string idToken)
    {
        var identity = await _googleVerifier.VerifyAsync(idToken);

        var email = identity.Email;
        if (string.IsNullOrEmpty(email))
            throw new InvalidOperationException("Google-акаунт не має email.");

        var fullName = identity.Name;

        var customer = await _customerRepository.GetByEmailAsync(email);
        if (customer is null)
        {
            customer = new Customer
            {
                FullName = string.IsNullOrEmpty(fullName) ? email : fullName,
                Email = email,
                GoogleUid = identity.Uid,
                PasswordHash = string.Empty
            };
            await _customerRepository.AddAsync(customer);
        }
        else if (customer.GoogleUid is null)
        {
            customer.GoogleUid = identity.Uid;
        }

        if (customer.IsBlocked)
            throw new InvalidOperationException("Обліковий запис заблоковано.");

        await _customerRepository.SaveChangesAsync();

        return customer.ToDto();
    }
}
