using System.Net;
using System.Net.Http.Json;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Tests.Integration.Infrastructure;
using Xunit;

namespace NewBalanceShop.Tests.Integration.Controllers;

public class CustomersControllerIntegrationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public CustomersControllerIntegrationTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<CustomerDto> RegisterAsync(string email)
    {
        var dto = new RegisterDto
        {
            FullName = "Тест Тестенко",
            Email = email,
            Password = "Passw0rd!",
            City = "Kyiv",
            Country = "UA",
            Phone = "+380",
        };
        var response = await _client.PostAsJsonAsync("/api/Auth/register", dto);
        return (await response.Content.ReadFromJsonAsync<CustomerDto>())!;
    }

    [Fact]
    public async Task GetAll_WithoutLogin_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/Customers");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_AsRegularCustomer_ReturnsForbidden()
    {
        await RegisterAsync("regular@example.com");

        var response = await _client.GetAsync("/api/Customers");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetById_AsOwner_ReturnsOwnProfile()
    {
        var me = await RegisterAsync("owner@example.com");

        var response = await _client.GetAsync($"/api/Customers/{me.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();
        Assert.Equal("owner@example.com", customer!.Email);
    }

    [Fact]
    public async Task GetById_AsAnotherCustomer_ReturnsForbidden()
    {
        var other = await RegisterAsync("other@example.com");
        await _client.PostAsync("/api/Auth/logout", null);
        await RegisterAsync("me@example.com");

        var response = await _client.GetAsync($"/api/Customers/{other.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_AsOwner_UpdatesProfile()
    {
        var me = await RegisterAsync("update@example.com");

        var response = await _client.PutAsJsonAsync($"/api/Customers/{me.Id}", new UpdateCustomerDto
        {
            FullName = "Нове Імʼя",
            Email = "update@example.com",
            City = "Lviv",
            Country = "UA",
            Phone = "+380999",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<CustomerDto>();
        Assert.Equal("Нове Імʼя", updated!.FullName);
        Assert.Equal("Lviv", updated.City);
    }

    [Fact]
    public async Task Update_WithEmailTakenByAnotherCustomer_ReturnsBadRequest()
    {
        await RegisterAsync("taken@example.com");
        await _client.PostAsync("/api/Auth/logout", null);
        var me = await RegisterAsync("free@example.com");

        var response = await _client.PutAsJsonAsync($"/api/Customers/{me.Id}", new UpdateCustomerDto
        {
            FullName = me.FullName,
            Email = "taken@example.com",
            City = me.City,
            Country = me.Country,
            Phone = me.Phone,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetOrders_AsOwner_ReturnsEmptyHistory()
    {
        var me = await RegisterAsync("orders@example.com");

        var response = await _client.GetAsync($"/api/Customers/{me.Id}/orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();
        Assert.Empty(orders!);
    }

    [Fact]
    public async Task SetBlocked_AsRegularCustomer_ReturnsForbidden()
    {
        var me = await RegisterAsync("blocktest@example.com");

        var response = await _client.PatchAsync($"/api/Customers/{me.Id}/block",
            JsonContent.Create(new { Blocked = true }));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AsRegularCustomer_ReturnsForbidden()
    {
        var me = await RegisterAsync("deletetest@example.com");

        var response = await _client.DeleteAsync($"/api/Customers/{me.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task LoginAsSeedAdminAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/login",
            new LoginDto { Email = "admin@newbalanceshop.com", Password = "Admin123!" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_AsAdmin_ReturnsAllCustomers()
    {
        await LoginAsSeedAdminAsync();

        var response = await _client.GetAsync("/api/Customers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var customers = await response.Content.ReadFromJsonAsync<List<CustomerDto>>();
        Assert.True(customers!.Count >= 2);
    }

    [Fact]
    public async Task SetBlocked_AsAdmin_BlocksTargetCustomer()
    {
        await LoginAsSeedAdminAsync();
        var response = await _client.PatchAsync("/api/Customers/1/block", JsonContent.Create(new { Blocked = true }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<CustomerDto>();
        Assert.True(updated!.IsBlocked);
    }

    [Fact]
    public async Task Delete_AsAdmin_RemovesTargetCustomer()
    {
        await LoginAsSeedAdminAsync();
        var response = await _client.DeleteAsync("/api/Customers/1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync("/api/Customers/1");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
