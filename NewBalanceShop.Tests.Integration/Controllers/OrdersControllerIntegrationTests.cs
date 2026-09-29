using System.Net;
using System.Net.Http.Json;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Tests.Integration.Infrastructure;
using Xunit;

namespace NewBalanceShop.Tests.Integration.Controllers;

public class OrdersControllerIntegrationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public OrdersControllerIntegrationTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetByCustomer_ReturnsOrdersForCustomer()
    {
        var orders = await _client.GetFromJsonAsync<List<OrderDto>>("/api/Orders?customerId=1");

        Assert.NotNull(orders);
        Assert.Empty(orders!);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/Orders/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithValidOrder_ReturnsCreated()
    {
        var dto = new CreateOrderDto
        {
            CustomerId = 1,
            Items = new List<CreateOrderItemDto> { new() { ProductId = 1, Quantity = 1 } }
        };

        var response = await _client.PostAsJsonAsync("/api/Orders", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.Equal(1, created!.CustomerId);
    }

    [Fact]
    public async Task Create_WithInvalidCustomer_ReturnsBadRequest()
    {
        var dto = new CreateOrderDto
        {
            CustomerId = 9999,
            Items = new List<CreateOrderItemDto> { new() { ProductId = 1, Quantity = 1 } }
        };

        var response = await _client.PostAsJsonAsync("/api/Orders", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithEmptyItems_ReturnsBadRequest()
    {
        var dto = new CreateOrderDto { CustomerId = 1, Items = new List<CreateOrderItemDto>() };

        var response = await _client.PostAsJsonAsync("/api/Orders", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ThenGetByCustomer_ReturnsCreatedOrder()
    {
        var dto = new CreateOrderDto
        {
            CustomerId = 1,
            Items = new List<CreateOrderItemDto> { new() { ProductId = 1, Quantity = 2 } }
        };
        await _client.PostAsJsonAsync("/api/Orders", dto);

        var orders = await _client.GetFromJsonAsync<List<OrderDto>>("/api/Orders?customerId=1");

        Assert.Single(orders!);
    }
}
