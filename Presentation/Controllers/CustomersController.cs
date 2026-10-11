using Microsoft.AspNetCore.Mvc;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;

namespace NewBalanceShop.Presentation.Controllers;

// FR-10/FR-11 — кабінет покупця (перегляд/редагування власних даних, історія замовлень);
// FR-12/FR-13 — адміністрування користувачів (список/блокування/видалення), лише для IsAdmin.
// Роль перевіряється по тій самій сесійній куці, що й логін (AuthController) — окремого
// JWT/ролевого middleware для навчального проєкту не заводимо. Forbid() тут не годиться:
// воно запускає ASP.NET Core auth challenge, а authentication-схему (cookie/JWT) ми не
// реєстрували, тож Forbid() падав у 500 замість 403 — повертаємо статус напряму.
/// <summary>Кабінет покупця (FR-10, FR-11) та адміністрування користувачів (FR-12, FR-13). Адмін-ендпоінти повертають 401 без входу та 403 без прав адміністратора.</summary>
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private const string CustomerIdSessionKey = "customerId";

    private readonly ICustomerService _customerService;
    private readonly IOrderService _orderService;

    public CustomersController(ICustomerService customerService, IOrderService orderService)
    {
        _customerService = customerService;
        _orderService = orderService;
    }

    private async Task<CustomerDto?> GetSessionCustomerAsync()
    {
        var id = HttpContext.Session.GetInt32(CustomerIdSessionKey);
        return id is null ? null : await _customerService.GetByIdAsync(id.Value);
    }

    private static ActionResult Denied(CustomerDto? me) =>
        me is null
            ? new UnauthorizedResult()
            : new StatusCodeResult(StatusCodes.Status403Forbidden);

    // GET api/customers — FR-12, лише адміністратор
    /// <summary>Список користувачів (лише адміністратор).</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
    {
        var me = await GetSessionCustomerAsync();
        if (me is null || !me.IsAdmin) return Denied(me);

        return Ok(await _customerService.GetAllAsync());
    }

    // GET api/customers/{id} — FR-10, власник кабінету або адміністратор
    /// <summary>Профіль (власник або адміністратор).</summary>
    /// <param name="id">Ключ покупця.</param>
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var me = await GetSessionCustomerAsync();
        if (me is null || (me.Id != id && !me.IsAdmin)) return Denied(me);

        var customer = await _customerService.GetByIdAsync(id);
        if (customer is null) return NotFound();

        return Ok(customer);
    }

    // PUT api/customers/{id} — FR-10, власник кабінету або адміністратор
    /// <summary>Оновлює профіль (власник або адміністратор).</summary>
    /// <param name="id">Ключ покупця.</param>
    /// <param name="dto">Нові дані.</param>
    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerDto>> Update(int id, UpdateCustomerDto dto)
    {
        var me = await GetSessionCustomerAsync();
        if (me is null || (me.Id != id && !me.IsAdmin)) return Denied(me);

        try
        {
            var updated = await _customerService.UpdateAsync(id, dto);
            if (updated is null) return NotFound();

            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET api/customers/{id}/orders — FR-11, тільки читання, власник кабінету або адміністратор
    /// <summary>Історія замовлень (власник або адміністратор), лише читання.</summary>
    /// <param name="id">Ключ покупця.</param>
    [HttpGet("{id}/orders")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders(int id)
    {
        var me = await GetSessionCustomerAsync();
        if (me is null || (me.Id != id && !me.IsAdmin)) return Denied(me);

        return Ok(await _orderService.GetByCustomerAsync(id));
    }

    // PATCH api/customers/{id}/block — FR-13, лише адміністратор
    /// <summary>Блокує або розблоковує покупця (лише адміністратор).</summary>
    /// <param name="id">Ключ покупця.</param>
    /// <param name="dto">Нове значення.</param>
    [HttpPatch("{id}/block")]
    public async Task<ActionResult<CustomerDto>> SetBlocked(int id, [FromBody] SetBlockedDto dto)
    {
        var me = await GetSessionCustomerAsync();
        if (me is null || !me.IsAdmin) return Denied(me);

        var updated = await _customerService.SetBlockedAsync(id, dto.Blocked);
        if (updated is null) return NotFound();

        return Ok(updated);
    }

    // DELETE api/customers/{id} — FR-13, лише адміністратор
    /// <summary>Видаляє покупця (лише адміністратор).</summary>
    /// <param name="id">Ключ покупця.</param>
    /// <returns>204 або 404.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var me = await GetSessionCustomerAsync();
        if (me is null || !me.IsAdmin) return Denied(me);

        var deleted = await _customerService.DeleteAsync(id);
        if (!deleted) return NotFound();

        return NoContent();
    }
}

/// <summary>Запит на блокування або розблокування.</summary>
public class SetBlockedDto
{
    public bool Blocked { get; set; }
}
