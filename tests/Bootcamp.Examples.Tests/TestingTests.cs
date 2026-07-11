using dev.kaldiroglu.bootcamp.testing;
using Xunit;

namespace Bootcamp.Examples.Tests;

public class TestingTests
{
    [Fact]
    public void AddsTwoNumbers_ArrangeActAssert()
    {
        var calc = new Calculator();          // arrange
        var result = calc.Add(2, 3);          // act
        Assert.Equal(5, result);              // assert
    }

    [Fact]
    public void DivideByZeroThrows()
    {
        Assert.Throws<DivideByZeroException>(() => new Calculator().Divide(1, 0));
    }

    /// A hand-written test double (a spy) isolates the unit under test.
    private sealed class FakeMailer : IMailer
    {
        public readonly List<(string To, string Body)> Sent = new();
        public void Send(string to, string body) => Sent.Add((to, body));
    }

    [Fact]
    public void SendsExactlyOneWelcomeMail()
    {
        var mailer = new FakeMailer();
        new WelcomeService(mailer).Welcome("ada@x.com", "Ada");
        Assert.Single(mailer.Sent);
        Assert.Equal("ada@x.com", mailer.Sent[0].To);
        Assert.Equal("Welcome, Ada!", mailer.Sent[0].Body);
    }
}
