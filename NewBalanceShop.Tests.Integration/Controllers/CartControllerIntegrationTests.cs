using System.Net;
using System.Net.Http.Json;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Tests.Integration.Infrastructure;
using Xunit;

namespace NewBalanceShop.Tests.Integration.Controllers;

// HandleCookies=true (дефолт WebApplicationFactory) тримає сесійну куку між
// запитами одного HttpClient — так само, як браузер покупця в реальному сценарії.
public class CartControllerIntegrationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public CartControllerIntegrationTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetCart_WhenEmpty_ReturnsEmptyArray()
    {
        var cart = await _client.GetFromJsonAsync<List<CartItemDto>>("/api/Cart");

        Assert.NotNull(cart);
        Assert.Empty(cart!);
    }

    [Fact]
    public async Task AddToCart_WithValidProduct_ReturnsUpdatedCart()
    {
        var response = await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Quantity = 2 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cart = await response.Content.ReadFromJsonAsync<List<CartItemDto>>();
        Assert.Single(cart!);
        Assert.Equal(2, cart![0].Quantity);
    }

    [Fact]
    public async Task AddToCart_WithSameProductTwice_IncreasesQuantity()
    {
        await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Quantity = 1 });
        var response = await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Quantity = 3 });

        var cart = await response.Content.ReadFromJsonAsync<List<CartItemDto>>();
        Assert.Single(cart!);
        Assert.Equal(4, cart![0].Quantity);
    }

    [Fact]
    public async Task AddToCart_WithNonExistingProduct_ReturnsNotFound()
    {
        var response = await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 9999, Quantity = 1 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveFromCart_RemovesItem()
    {
        await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Quantity = 1 });

        var response = await _client.DeleteAsync("/api/Cart/1");

        var cart = await response.Content.ReadFromJsonAsync<List<CartItemDto>>();
        Assert.Empty(cart!);
    }

    [Fact]
    public async Task Checkout_WithEmptyCart_ReturnsBadRequest()
    {
        var response = await _client.PostAsync("/api/Cart/checkout?customerId=1", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Checkout_WithItemsInCart_CreatesOrderAndClearsCart()
    {
        await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Quantity = 2 });

        var checkoutResponse = await _client.PostAsync("/api/Cart/checkout?customerId=1", null);
        Assert.Equal(HttpStatusCode.OK, checkoutResponse.StatusCode);

        var order = await checkoutResponse.Content.ReadFromJsonAsync<OrderDto>();
        Assert.Equal(1, order!.CustomerId);
        Assert.Single(order.Items);

        var cartAfter = await _client.GetFromJsonAsync<List<CartItemDto>>("/api/Cart");
        Assert.Empty(cartAfter!);
    }
}
