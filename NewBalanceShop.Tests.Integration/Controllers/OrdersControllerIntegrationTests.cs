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

    private static CreateOrderDto OneItemOrder(int customerId = 0, int quantity = 1) => new()
    {
        CustomerId = customerId,
        Items = new List<CreateOrderItemDto> { new() { ProductId = 1, Quantity = quantity } }
    };

    private async Task<CustomerDto> RegisterAsync(string email, HttpClient? client = null)
    {
        var response = await (client ?? _client).PostAsJsonAsync("/api/Auth/register", new RegisterDto
        {
            FullName = "Покупець", Email = email, Password = "Passw0rd!"
        });
        return (await response.Content.ReadFromJsonAsync<CustomerDto>())!;
    }

    [Fact]
    public async Task GetByCustomer_WithoutLogin_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/Orders");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetByCustomer_WhenSignedIn_ReturnsOwnOrders()
    {
        await RegisterAsync("orders-owner@example.com");

        var orders = await _client.GetFromJsonAsync<List<OrderDto>>("/api/Orders");

        Assert.NotNull(orders);
        Assert.Empty(orders!);
    }

    [Fact]
    public async Task GetById_WithoutLogin_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/Orders/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNotFound()
    {
        await RegisterAsync("orders-404@example.com");

        var response = await _client.GetAsync("/api/Orders/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_AsGuest_ReturnsCreatedForGuestCustomer()
    {
        var response = await _client.PostAsJsonAsync("/api/Orders", OneItemOrder());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.Equal(1, created!.CustomerId);
    }

    [Fact]
    public async Task Create_WhenSignedIn_IgnoresCustomerIdFromBody()
    {
        var me = await RegisterAsync("orders-spoof@example.com");

        var response = await _client.PostAsJsonAsync("/api/Orders", OneItemOrder(customerId: 2));

        var created = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.Equal(me.Id, created!.CustomerId);
    }

    [Fact]
    public async Task Create_WithEmptyItems_ReturnsBadRequest()
    {
        var dto = new CreateOrderDto { Items = new List<CreateOrderItemDto>() };

        var response = await _client.PostAsJsonAsync("/api/Orders", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithTooLargeQuantity_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/Orders", OneItemOrder(quantity: 1000));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ThenGetByCustomer_ReturnsCreatedOrder()
    {
        await RegisterAsync("orders-history@example.com");
        await _client.PostAsJsonAsync("/api/Orders", OneItemOrder(quantity: 2));

        var orders = await _client.GetFromJsonAsync<List<OrderDto>>("/api/Orders");

        Assert.Single(orders!);
    }

    // BOLA/IDOR: покупець B не бачить замовлення покупця A ні за списком, ні за ключем
    [Fact]
    public async Task ForeignCustomer_CannotReadOrAccessOthersOrders()
    {
        var owner = await RegisterAsync("bola-owner@example.com");
        var created = await (await _client.PostAsJsonAsync("/api/Orders", OneItemOrder())).Content.ReadFromJsonAsync<OrderDto>();

        using var attacker = _factory.CreateClient();
        await RegisterAsync("bola-attacker@example.com", attacker);

        Assert.Equal(HttpStatusCode.Forbidden, (await attacker.GetAsync($"/api/Orders?customerId={owner.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await attacker.GetAsync($"/api/Orders/{created!.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"/api/Orders/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task Admin_CanReadAnyCustomersOrders()
    {
        var owner = await RegisterAsync("bola-admin-owner@example.com");
        var created = await (await _client.PostAsJsonAsync("/api/Orders", OneItemOrder())).Content.ReadFromJsonAsync<OrderDto>();

        using var admin = _factory.CreateClient();
        _factory.SetTestAdminPassword();
        var login = await admin.PostAsJsonAsync("/api/Auth/login",
            new LoginDto { Email = "admin@newbalanceshop.com", Password = CustomWebApplicationFactory.TestAdminPassword });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var orders = await admin.GetFromJsonAsync<List<OrderDto>>($"/api/Orders?customerId={owner.Id}");
        Assert.Single(orders!);
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync($"/api/Orders/{created!.Id}")).StatusCode);
    }
}
