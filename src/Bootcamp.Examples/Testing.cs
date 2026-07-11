// ◀ Slides: Deck 07 Developer Testing — AAA + test doubles
namespace dev.kaldiroglu.bootcamp.testing;

// Topic 07 — Developer Testing: a unit under test and a test double.

public sealed class Calculator
{
    public int Add(int a, int b) => a + b;

    public int Divide(int a, int b)
    {
        if (b == 0)
        {
            throw new DivideByZeroException();
        }
        return a / b;
    }
}

/// A collaborator we don't want to really invoke in a unit test.
public interface IMailer
{
    void Send(string to, string body);
}

/// The unit under test: greets a new member by sending a welcome mail.
public sealed class WelcomeService(IMailer mailer)
{
    public void Welcome(string email, string name) =>
        mailer.Send(email, $"Welcome, {name}!");
}
