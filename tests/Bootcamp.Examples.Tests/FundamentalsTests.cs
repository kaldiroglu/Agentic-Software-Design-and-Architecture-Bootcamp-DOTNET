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
    public void DemeterBothReturnTheSameCity()
    {
        var order = new DemeterOrder("o1",
            new DemeterCustomer("Ada", new Address("1 Main St", "Istanbul")));
        Assert.Equal("Istanbul", new ShippingLabelSmell().CityFor(order));
        Assert.Equal("Istanbul", order.ShippingCity());
    }
}
