using Moq;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Services;
using NewBalanceShop.Domain.Entities;
using NewBalanceShop.Domain.Interfaces;
using Xunit;

namespace NewBalanceShop.Tests.Unit.Services;

public class AuthServiceTests
{
    private readonly Mock<ICustomerRepository> _repo = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_repo.Object);
    }

    private static RegisterDto MakeRegisterDto() => new()
    {
        FullName = "Тест Тестенко",
        Email = "test@example.com",
        Password = "Passw0rd!",
        City = "Kyiv",
        Country = "UA",
        Phone = "+380000000000"
    };

    [Fact]
    public async Task RegisterAsync_WithNewEmail_CreatesCustomer()
    {
        _repo.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync((Customer?)null);

        var result = await _sut.RegisterAsync(MakeRegisterDto());

        Assert.Equal("test@example.com", result.Email);
        _repo.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsInvalidOperationException()
    {
        _repo.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync(new Customer { Email = "test@example.com" });

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.RegisterAsync(MakeRegisterDto()));
        _repo.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_HashesPassword()
    {
        _repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((Customer?)null);
        Customer? captured = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<Customer>())).Callback<Customer>(c => captured = c).Returns(Task.CompletedTask);

        await _sut.RegisterAsync(MakeRegisterDto());

        Assert.NotNull(captured);
        Assert.NotEqual("Passw0rd!", captured!.PasswordHash);
        Assert.True(PasswordHasher.Verify("Passw0rd!", captured.PasswordHash));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsCustomer()
    {
        var customer = new Customer { Id = 1, Email = "test@example.com", PasswordHash = PasswordHasher.Hash("Passw0rd!") };
        _repo.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync(customer);

        var result = await _sut.LoginAsync(new LoginDto { Email = "test@example.com", Password = "Passw0rd!" });

        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsInvalidOperationException()
    {
        var customer = new Customer { Id = 1, Email = "test@example.com", PasswordHash = PasswordHasher.Hash("Passw0rd!") };
        _repo.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync(customer);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.LoginAsync(new LoginDto { Email = "test@example.com", Password = "wrong" }));
    }

    [Fact]
    public async Task LoginAsync_WithNonExistingEmail_ThrowsInvalidOperationException()
    {
        _repo.Setup(r => r.GetByEmailAsync("missing@example.com")).ReturnsAsync((Customer?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.LoginAsync(new LoginDto { Email = "missing@example.com", Password = "x" }));
    }

    [Fact]
    public async Task LoginAsync_WithBlockedCustomer_ThrowsInvalidOperationException()
    {
        var customer = new Customer { Id = 1, Email = "b@example.com", PasswordHash = PasswordHasher.Hash("pw"), IsBlocked = true };
        _repo.Setup(r => r.GetByEmailAsync("b@example.com")).ReturnsAsync(customer);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.LoginAsync(new LoginDto { Email = "b@example.com", Password = "pw" }));
        Assert.Contains("заблоковано", ex.Message);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsCustomer()
    {
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Customer { Id = 1, Email = "a@b.com" });

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
    public async Task GoogleLoginAsync_WhenFirebaseNotConfigured_ThrowsInvalidOperationException()
    {
        // у тестовому процесі FirebaseApp.Create() ніколи не викликається,
        // тож FirebaseAuth.DefaultInstance завжди null — саме цю (реальну!) поведінку й перевіряємо
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.GoogleLoginAsync("any-token"));
        Assert.Contains("Firebase не налаштовано", ex.Message);
    }
}
