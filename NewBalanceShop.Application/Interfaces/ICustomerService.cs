using NewBalanceShop.Application.DTO;

namespace NewBalanceShop.Application.Interfaces;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllAsync();
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto dto);
    Task<CustomerDto?> SetBlockedAsync(int id, bool blocked);
    Task<bool> DeleteAsync(int id);
}
