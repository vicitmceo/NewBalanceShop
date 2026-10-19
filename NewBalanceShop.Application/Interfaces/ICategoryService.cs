using NewBalanceShop.Application.DTO;

namespace NewBalanceShop.Application.Interfaces;

/// <summary>Сценарії роботи з категоріями товарів.</summary>
public interface ICategoryService
{
    /// <summary>Усі категорії (кешуються на короткий час, бо змінюються рідко).</summary>
    Task<List<CategoryDto>> GetAllAsync();
}
