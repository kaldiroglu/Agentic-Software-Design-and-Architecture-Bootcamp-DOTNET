using dev.kaldiroglu.bootcamp.fundamentals.password;
using Xunit;

namespace Bootcamp.Examples.Tests;

public class PasswordTests
{
    [Fact]
    public void PasswordValueObjectKeepsAllItsBehaviourTogether()
    {
        var password = Password.Of("Sup3r-Secret!!");   // 14 chars: upper, lower, digit, symbol

        Assert.False(Password.IsValid("password"));
        Assert.Throws<ArgumentException>(() => Password.Of("short"));

        Assert.True(password.Matches("Sup3r-Secret!!"));
        Assert.False(password.Matches("Sup3r-Secret!?"));

        Assert.Equal("••••••••••••••", password.Masked());
        Assert.Equal("••••••••••••!!", password.MaskedShowingLast(2));
        Assert.Equal(password.Masked(), password.ToString());

        var random = Password.Random(16);
        Assert.Equal(16, random.Masked().Length);
        Assert.NotEqual(random, Password.Random(16));
    }

    [Fact]
    public void PasswordHasherStoresAndVerifiesWithoutKeepingThePlaintext()
    {
        IPasswordHasher hasher = new Pbkdf2PasswordHasher();
        var stored = hasher.Hash(Password.Of("Sup3r-Secret!!"));

        Assert.True(hasher.Verify(Password.Of("Sup3r-Secret!!"), stored));
        Assert.False(hasher.Verify(Password.Of("Wr0ng-Secret!!"), stored));

        // salted: the same password hashes differently each time, yet still verifies.
        var again = hasher.Hash(Password.Of("Sup3r-Secret!!"));
        Assert.NotEqual(stored.Encoded, again.Encoded);
        Assert.True(hasher.Verify(Password.Of("Sup3r-Secret!!"), again));
    }
}
