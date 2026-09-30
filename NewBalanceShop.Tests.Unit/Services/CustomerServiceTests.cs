using Moq;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Services;
using NewBalanceShop.Domain.Entities;
using NewBalanceShop.Domain.Interfaces;
using Xunit;

namespace NewBalanceShop.Tests.Unit.Services;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _repo = new();
    private readonly CustomerService _sut;

    public CustomerServiceTests()
    {
        _sut = new CustomerService(_repo.Object);
    }

    private static Customer MakeCustomer(int id = 1, string email = "t@t.com") => new()
    {
        Id = id,
        FullName = "Тест",
        Email = email,
        City = "Kyiv",
        Country = "UA",
        Phone = "+380",
        PasswordHash = "hash"
    };

    private static UpdateCustomerDto MakeUpdateDto(string email = "t@t.com") => new()
    {
        FullName = "Оновлено",
        Email = email,
        City = "Lviv",
        Country = "UA",
        Phone = "+380999"
    };

    [Fact]
    public async Task GetAllAsync_ReturnsAllCustomers()
    {
        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Customer> { MakeCustomer(1), MakeCustomer(2) });

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsCustomer()
    {
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeCustomer(1));

        var result = await _sut.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Customer?)null);

        var result = await _sut.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingCustomer_UpdatesFieldsAndReturnsDto()
    {
        var customer = MakeCustomer(1, "t@t.com");
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);

        var result = await _sut.UpdateAsync(1, MakeUpdateDto());

        Assert.NotNull(result);
        Assert.Equal("Оновлено", customer.FullName);
        Assert.Equal("Lviv", customer.City);
        _repo.Verify(r => r.Update(customer), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingCustomer_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Customer?)null);

        var result = await _sut.UpdateAsync(999, MakeUpdateDto());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithEmailTakenByAnotherCustomer_ThrowsInvalidOperationException()
    {
        var customer = MakeCustomer(1, "old@t.com");
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
        _repo.Setup(r => r.GetByEmailAsync("taken@t.com")).ReturnsAsync(MakeCustomer(2, "taken@t.com"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateAsync(1, MakeUpdateDto("taken@t.com")));
        _repo.Verify(r => r.Update(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_KeepingSameEmail_DoesNotThrow()
    {
        var customer = MakeCustomer(1, "t@t.com");
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);

        var result = await _sut.UpdateAsync(1, MakeUpdateDto("t@t.com"));

        Assert.NotNull(result);
        _repo.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SetBlockedAsync_WithExistingCustomer_SetsFlagAndReturnsDto()
    {
        var customer = MakeCustomer(1);
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);

        var result = await _sut.SetBlockedAsync(1, true);

        Assert.NotNull(result);
        Assert.True(customer.IsBlocked);
        Assert.True(result!.IsBlocked);
    }

    [Fact]
    public async Task SetBlockedAsync_WithNonExistingCustomer_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Customer?)null);

        var result = await _sut.SetBlockedAsync(999, true);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingCustomer_RemovesAndReturnsTrue()
    {
        var customer = MakeCustomer(1);
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);

        var result = await _sut.DeleteAsync(1);

        Assert.True(result);
        _repo.Verify(r => r.Remove(customer), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingCustomer_ReturnsFalse()
    {
        _repo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Customer?)null);

        var result = await _sut.DeleteAsync(999);

        Assert.False(result);
    }
}
