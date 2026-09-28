using NewBalanceShop.Application.Services;
using Xunit;

namespace NewBalanceShop.Tests.Unit.Services;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_ProducesDifferentHashForSamePassword()
    {
        var hash1 = PasswordHasher.Hash("Passw0rd!");
        var hash2 = PasswordHasher.Hash("Passw0rd!");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = PasswordHasher.Hash("Passw0rd!");

        Assert.True(PasswordHasher.Verify("Passw0rd!", hash));
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = PasswordHasher.Hash("Passw0rd!");

        Assert.False(PasswordHasher.Verify("WrongPassword", hash));
    }

    [Fact]
    public void Verify_WithTamperedHash_ReturnsFalse()
    {
        var hash = PasswordHasher.Hash("Passw0rd!");
        var tampered = hash[..^4] + "AAAA";

        Assert.False(PasswordHasher.Verify("Passw0rd!", tampered));
    }

    [Fact]
    public void Verify_WithMalformedStoredHash_ReturnsFalse()
    {
        Assert.False(PasswordHasher.Verify("Passw0rd!", "not-a-valid-hash"));
    }
}
