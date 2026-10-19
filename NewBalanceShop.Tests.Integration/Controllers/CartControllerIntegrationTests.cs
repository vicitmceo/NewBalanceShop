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
        var response = await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Size = "42", Quantity = 2 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cart = await response.Content.ReadFromJsonAsync<List<CartItemDto>>();
        Assert.Single(cart!);
        Assert.Equal(2, cart![0].Quantity);
    }

    [Fact]
    public async Task AddToCart_WithSameProductTwice_IncreasesQuantity()
    {
        await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Size = "42", Quantity = 1 });
        var response = await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Size = "42", Quantity = 3 });

        var cart = await response.Content.ReadFromJsonAsync<List<CartItemDto>>();
        Assert.Single(cart!);
        Assert.Equal(4, cart![0].Quantity);
    }

    [Fact]
    public async Task AddToCart_WithNonExistingProduct_ReturnsNotFound()
    {
        var response = await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 9999, Size = "42", Quantity = 1 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveFromCart_RemovesItem()
    {
        await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Size = "42", Quantity = 1 });

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
    public async Task Checkout_IgnoresCustomerIdFromQuery()
    {
        await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Size = "42", Quantity = 1 });

        var response = await _client.PostAsync("/api/Cart/checkout?customerId=2", null);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.Equal(1, order!.CustomerId);
    }

    [Fact]
    public async Task Checkout_WhenSignedIn_CreatesOrderForThatCustomer()
    {
        var register = await _client.PostAsJsonAsync("/api/Auth/register", new RegisterDto
        {
            FullName = "Покупець", Email = "cart-buyer@example.com", Password = "Passw0rd!"
        });
        var me = await register.Content.ReadFromJsonAsync<CustomerDto>();
        await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Size = "42", Quantity = 1 });

        var response = await _client.PostAsync("/api/Cart/checkout", null);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.Equal(me!.Id, order!.CustomerId);
    }

    [Fact]
    public async Task Checkout_WithItemsInCart_CreatesOrderAndClearsCart()
    {
        await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Size = "42", Quantity = 2 });

        var checkoutResponse = await _client.PostAsync("/api/Cart/checkout?customerId=1", null);
        Assert.Equal(HttpStatusCode.OK, checkoutResponse.StatusCode);

        var order = await checkoutResponse.Content.ReadFromJsonAsync<OrderDto>();
        Assert.Equal(1, order!.CustomerId);
        Assert.Single(order.Items);

        var cartAfter = await _client.GetFromJsonAsync<List<CartItemDto>>("/api/Cart");
        Assert.Empty(cartAfter!);
    }

    [Fact]
    public async Task AddToCart_MoreThanStock_ReturnsBadRequestWithMessage()
    {
        var response = await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 2, Size = "42", Quantity = 9999 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Недостатньо", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task AddToCart_TwiceBeyondStock_IsRejected()
    {
        await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 2, Size = "42", Quantity = 10 });

        var response = await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 2, Size = "42", Quantity = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetQuantity_ChangesQuantity_AndRejectsOverStock()
    {
        await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Size = "42", Quantity = 1 });

        var ok = await _client.PutAsJsonAsync("/api/Cart/1", new SetCartQuantityDto { Quantity = 5 });
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        var cart = await ok.Content.ReadFromJsonAsync<List<CartItemDto>>();
        Assert.Equal(5, cart![0].Quantity);

        var tooMany = await _client.PutAsJsonAsync("/api/Cart/1", new SetCartQuantityDto { Quantity = 99 });
        Assert.Equal(HttpStatusCode.BadRequest, tooMany.StatusCode);
    }

    [Fact]
    public async Task SetQuantity_ForProductNotInCart_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync("/api/Cart/1", new SetCartQuantityDto { Quantity = 2 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddToCart_WithoutSizeWhenSeveralExist_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Quantity = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("розмір", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task AddToCart_WithUnknownSize_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Size = "99", Quantity = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddToCart_SingleSizeProduct_DoesNotRequireSize()
    {
        var response = await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 8, Quantity = 1 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cart = await response.Content.ReadFromJsonAsync<List<CartItemDto>>();
        Assert.Equal("One size", cart![0].Size);
    }

    [Fact]
    public async Task AddToCart_DifferentSizesOfSameProduct_AreSeparateLines()
    {
        await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Size = "41", Quantity = 1 });
        var response = await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Size = "43", Quantity = 2 });

        var cart = await response.Content.ReadFromJsonAsync<List<CartItemDto>>();
        Assert.Equal(2, cart!.Count);
        Assert.Equal(new[] { "41", "43" }, cart.Select(i => i.Size).OrderBy(x => x));
    }

    [Fact]
    public async Task RemoveFromCart_WithSize_RemovesOnlyThatSize()
    {
        await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Size = "41", Quantity = 1 });
        await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Size = "43", Quantity = 1 });

        var response = await _client.DeleteAsync("/api/Cart/1?size=41");

        var cart = await response.Content.ReadFromJsonAsync<List<CartItemDto>>();
        Assert.Single(cart!);
        Assert.Equal("43", cart![0].Size);
    }

    [Fact]
    public async Task Checkout_StoresChosenSizeInOrder()
    {
        await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Size = "44", Quantity = 1 });

        var response = await _client.PostAsync("/api/Cart/checkout", null);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.Equal("44", order!.Items[0].Size);
    }

    [Fact]
    public async Task Checkout_WithSeveralProducts_CreatesOrderWithAllLinesAndDecreasesStock()
    {
        await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Size = "42", Quantity = 2 });
        await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 1, Size = "43", Quantity = 1 });
        await _client.PostAsJsonAsync("/api/Cart", new AddToCartDto { ProductId = 2, Size = "42", Quantity = 1 });
        var before1 = (await _client.GetFromJsonAsync<ProductDto>("/api/Products/1"))!.Stock;
        var before2 = (await _client.GetFromJsonAsync<ProductDto>("/api/Products/2"))!.Stock;

        var response = await _client.PostAsync("/api/Cart/checkout", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.Equal(3, order!.Items.Count);
        Assert.Equal(3299m * 3 + 4199m, order.TotalPrice);
        Assert.Equal(before1 - 3, (await _client.GetFromJsonAsync<ProductDto>("/api/Products/1"))!.Stock);
        Assert.Equal(before2 - 1, (await _client.GetFromJsonAsync<ProductDto>("/api/Products/2"))!.Stock);
    }
}
