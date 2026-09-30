using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;
using NewBalanceShop.Application.Mapping;
using NewBalanceShop.Domain.Interfaces;

namespace NewBalanceShop.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<List<CustomerDto>> GetAllAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        return customers.Select(c => c.ToDto()).ToList();
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        return customer?.ToDto();
    }

    public async Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto dto)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer is null) return null;

        if (!string.Equals(customer.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _customerRepository.GetByEmailAsync(dto.Email);
            if (existing is not null && existing.Id != id)
                throw new InvalidOperationException($"Email '{dto.Email}' вже використовується іншим покупцем.");
        }

        customer.FullName = dto.FullName;
        customer.Email = dto.Email;
        customer.City = dto.City;
        customer.Country = dto.Country;
        customer.Phone = dto.Phone;

        _customerRepository.Update(customer);
        await _customerRepository.SaveChangesAsync();

        return customer.ToDto();
    }

    public async Task<CustomerDto?> SetBlockedAsync(int id, bool blocked)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer is null) return null;

        customer.IsBlocked = blocked;
        _customerRepository.Update(customer);
        await _customerRepository.SaveChangesAsync();

        return customer.ToDto();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer is null) return false;

        _customerRepository.Remove(customer);
        await _customerRepository.SaveChangesAsync();
        return true;
    }
}
