using Microsoft.AspNetCore.Mvc;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;

namespace NewBalanceShop.Presentation.Controllers;

/// <summary>Перегляд і створення замовлень.</summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>Замовлення за ключем.</summary>
    /// <param name="id">Ключ замовлення.</param>
    /// <returns>200 або 404.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order is null) return NotFound();

        return Ok(order);
    }

    /// <summary>Замовлення покупця.</summary>
    /// <param name="customerId">Ключ покупця.</param>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByCustomer([FromQuery] int customerId)
    {
        var orders = await _orderService.GetByCustomerAsync(customerId);
        return Ok(orders);
    }

    /// <summary>Створює замовлення.</summary>
    /// <param name="dto">Покупець і позиції.</param>
    /// <returns>201 і замовлення або 400.</returns>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

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
