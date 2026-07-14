using dev.kaldiroglu.bootcamp.fundamentals;
using Xunit;

namespace Bootcamp.Examples.Tests;

public class FundamentalsTests
{
    [Fact]
    public void FocusedClassesEachDoOneJob()
    {
        var repo = new UserRepository();
        repo.Save("u1", "Ada");
        Assert.Equal("Ada", repo.FindName("u1"));
        Assert.Equal(20.0, new TaxCalculator(0.20).TaxOn(100.0), 3);
        Assert.Contains("a@b.com", new EmailComposer().Compose("a@b.com", "Hi", "Body"));
    }

    [Fact]
    public void LooselyCoupledReportAcceptsAnyDatabase()
    {
        var db = new InMemoryDatabase(new[] { "a", "b", "c" });
        Assert.Equal(3, new Report(db).RowCount());
        Assert.Equal(1, new ReportSmell().RowCount());
    }

    [Fact]
    public void LeakyExposesItsListWhileSealedHidesIt()
    {
        var item = new Item("book");

        // Leaky: callers mutate X through the leaked list — even bypassing X entirely.
        var lx = new Leaky.X();
        new Leaky.Y(lx).Add(item);
        Assert.Equal(1, lx.Count);
        lx.GetItems().Add(new Item("smuggled"));   // no X method called — invariant bypassed
        Assert.Equal(2, lx.Count);
        new Leaky.Z(lx).Remove(item);
        Assert.Equal(1, lx.Count);

        // Sealed: same collaboration, but the list is never exposed — X stays in control.
        var sx = new Sealed.X();
        new Sealed.Y(sx).Add(item);
        Assert.Equal(1, sx.Count);
        new Sealed.Z(sx).Remove(item);
        Assert.Equal(0, sx.Count);
    }

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
    public void MessageCouplingLetsUsInjectAndVerifyAnyMailer()
    {
        var mailer = new RecordingMailer();
        new Orders(mailer).Place(new MailOrder("o1"));
        Assert.Single(mailer.Sent);
        Assert.Equal("Receipt for order o1", mailer.Sent[0]);
        // Programming to an implementation leaves no seam — it just runs.
        Assert.Null(Record.Exception(() => new OrdersSmell().Place(new MailOrder("o2"))));
    }

    [Fact]
    public void SubtypingSmellAndComposedFixAgree()
    {
        var smell = new AuditLedgerSmell();
        smell.Add(10);
        smell.Add(32);
        Assert.Equal(42, smell.AuditTotal());

        var ledger = new Ledger();
        ledger.Add(10);
        ledger.Add(32);
        Assert.Equal(42, new AuditLedger(ledger).AuditTotal());
    }

    [Fact]
    public void DemeterBothReturnTheSameCity()
    {
        var order = new DemeterOrder("o1",
            new DemeterCustomer("Ada", new Address("1 Main St", "Istanbul")));
        Assert.Equal("Istanbul", new ShippingLabelSmell().CityFor(order));
        Assert.Equal("Istanbul", order.ShippingCity());
    }
}
