using Microsoft.AspNetCore.Mvc;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;
using NewBalanceShop.Domain.Entities;
using NewBalanceShop.Presentation.Security;

namespace NewBalanceShop.Presentation.Controllers;

/// <summary>Перегляд і створення замовлень.</summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;

    public OrdersController(IOrderService orderService, ICustomerService customerService)
    {
        _orderService = orderService;
        _customerService = customerService;
    }

    /// <summary>Замовлення за ключем (власник або адміністратор).</summary>
    /// <param name="id">Ключ замовлення.</param>
    /// <returns>200; 404, якщо замовлення немає або воно чуже (існування чужих замовлень не розкриваємо).</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var me = await SessionAuth.GetCurrentAsync(HttpContext, _customerService);
        if (me is null) return Unauthorized();

        var order = await _orderService.GetByIdAsync(id);
        if (order is null || (order.CustomerId != me.Id && !me.IsAdmin)) return NotFound();

        return Ok(order);
    }

    /// <summary>Замовлення поточного покупця; адміністратор може вказати customerId.</summary>
    /// <param name="customerId">Лише для адміністратора.</param>
    /// <returns>200, 401 без входу, 403 при спробі дивитись чужі замовлення.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByCustomer([FromQuery] int? customerId)
    {
        var me = await SessionAuth.GetCurrentAsync(HttpContext, _customerService);
        if (me is null) return Unauthorized();

        var target = customerId ?? me.Id;
        if (target != me.Id && !me.IsAdmin) return StatusCode(StatusCodes.Status403Forbidden);

        return Ok(await _orderService.GetByCustomerAsync(target));
    }

    /// <summary>Створює замовлення.</summary>
    /// <param name="dto">Позиції (поле customerId ігнорується: покупець береться із сесії).</param>
    /// <returns>201 і замовлення або 400.</returns>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // покупця беремо із сесії, а не з тіла запиту (захист від BOLA/IDOR)
        var me = await SessionAuth.GetCurrentAsync(HttpContext, _customerService);
        dto.CustomerId = me?.Id ?? Customer.GuestId;

        try
        {
            var created = await _orderService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
