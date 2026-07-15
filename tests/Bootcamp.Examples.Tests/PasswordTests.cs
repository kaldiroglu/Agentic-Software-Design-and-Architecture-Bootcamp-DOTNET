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

    [Fact]
    public void FormatterAndFactorySplitPasswordResponsibilities()
    {
        var formatter = new PasswordFormatter();
        var factory = new PasswordFactory();

        var password = Password.Of("Sup3r-Secret!!");   // 14 chars
        Assert.Equal("[redacted: 14 chars]", formatter.RedactedForLog(password));
        Assert.Equal("Su••••••••••••", formatter.ShowingFirst(password, 2));

        Assert.Equal(16, factory.Temporary().Masked().Length);
        Assert.Equal(24, factory.ResetToken().Masked().Length);
        Assert.NotEqual(factory.Temporary(), factory.Temporary());   // each is unique
        Assert.True(factory.FromRaw("Sup3r-Secret!!").Matches("Sup3r-Secret!!"));
        Assert.Throws<ArgumentException>(() => factory.FromRaw("weak"));
    }
}
