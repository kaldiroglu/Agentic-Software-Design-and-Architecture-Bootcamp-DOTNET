using dev.kaldiroglu.bootcamp.secure;
using Xunit;

namespace Bootcamp.Examples.Tests;

public class SecureTests
{
    private const string Attack = "' OR '1'='1";

    [Fact]
    public void Concatenation_LetsTheAttackerRewriteTheQuery()
    {
        var sql = new VulnerableUserDao().BuildQuery(Attack);
        Assert.Contains("OR '1'='1", sql);
    }

    [Fact]
    public void Parameterization_KeepsInputAsData()
    {
        var q = new SafeUserDao().BuildQuery(Attack);
        Assert.Equal("SELECT * FROM users WHERE name = ?", q.Sql);
        Assert.DoesNotContain("OR", q.Sql);
        Assert.Equal(Attack, q.Parameters[0]);
    }

    [Fact]
    public void SafeMessage_HidesInternalsThatTheSmellExposes()
    {
        var responder = new ErrorResponder();
        var boom = new InvalidOperationException("column 'ssn' at db-prod-7:5432");
        Assert.Contains("ssn", responder.SmellUserMessage(boom));
        Assert.DoesNotContain("ssn", responder.SafeUserMessage(boom));
        Assert.Contains("ssn", responder.InternalLog(boom));
    }

    [Fact]
    public void Secret_ComesFromConfigAndFailsLoudlyWhenMissing()
    {
        var ok = new Secrets(new Dictionary<string, string> { ["API_KEY"] = "sk_test" });
        Assert.Equal("sk_test", ok.ApiKey());
        var missing = new Secrets(new Dictionary<string, string>());
        Assert.Throws<InvalidOperationException>(() => missing.ApiKey());
    }
}
