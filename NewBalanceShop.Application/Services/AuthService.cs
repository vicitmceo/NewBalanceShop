using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;
using NewBalanceShop.Application.Mapping;
using NewBalanceShop.Domain.Entities;
using NewBalanceShop.Domain.Interfaces;

namespace NewBalanceShop.Application.Services;

public class AuthService : IAuthService
{
    private readonly ICustomerRepository _customerRepository;

    public AuthService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
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
}
