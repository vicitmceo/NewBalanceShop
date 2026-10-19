using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;
using NewBalanceShop.Application.Mapping;
using NewBalanceShop.Domain.Interfaces;

namespace NewBalanceShop.Application.Services;

/// <summary>Реалізація <see cref="ICustomerService"/>.</summary>
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
        var customer = await _customerRepository.GetByIdNoTrackingAsync(id);
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

        if (id == Domain.Entities.Customer.GuestId)
            throw new InvalidOperationException("Службового покупця «Гість» видаляти не можна.");

        // історію замовлень не губимо: видалення покупця з замовленнями заборонене (блокуйте його)
        if (await _customerRepository.HasOrdersAsync(id))
            throw new InvalidOperationException("У покупця є замовлення, тому його не можна видалити. Заблокуйте обліковий запис.");

        _customerRepository.Remove(customer);
        await _customerRepository.SaveChangesAsync();
        return true;
    }
}
