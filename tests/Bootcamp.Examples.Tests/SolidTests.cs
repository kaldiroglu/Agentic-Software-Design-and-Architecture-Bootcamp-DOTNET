using Xunit;
using Srp = dev.kaldiroglu.bootcamp.solid.srp;
using Ocp = dev.kaldiroglu.bootcamp.solid.ocp;
using LspBad = dev.kaldiroglu.bootcamp.solid.lsp.violation;
using LspGood = dev.kaldiroglu.bootcamp.solid.lsp.correct;
using Isp = dev.kaldiroglu.bootcamp.solid.isp;
using Dip = dev.kaldiroglu.bootcamp.solid.dip;

namespace Bootcamp.Examples.Tests;

public class SolidTests
{
    [Fact]
    public void Srp_EachClassOwnsOneConcern()
    {
        var invoice = new Srp.Invoice("INV-1", 100.0);
        Assert.Equal(100.0, invoice.Total(), 3);
        Assert.Contains("INV-1", new Srp.InvoiceRepository().Save(invoice));
        Assert.Contains("100", new Srp.InvoiceRenderer().ToHtml(invoice));
    }

    [Fact]
    public void Ocp_NewRuleIsANewClassNotAnEdit()
    {
        Ocp.IPricing book = new Ocp.BookPricing();
        Ocp.IPricing food = new Ocp.FoodPricing();
        Assert.Equal(new Ocp.PriceCalculatorSmell().Price("book", 50), book.Price(50), 3);
        Assert.Equal(new Ocp.PriceCalculatorSmell().Price("food", 50), food.Price(50), 3);
    }

    [Fact]
    public void Ocp_EachRoleIsASubclassNotACaseInASwitch()
    {
        Assert.Equal(new Ocp.EmployeePaySmell(Ocp.EmployeePaySmell.ENGINEER, 100).Pay(),
            new Ocp.Engineer(100).Pay(), 3);
        Assert.Equal(new Ocp.EmployeePaySmell(Ocp.EmployeePaySmell.SALESMAN, 100).Pay(),
            new Ocp.Salesman(100).Pay(), 3);
        Assert.Equal(new Ocp.EmployeePaySmell(Ocp.EmployeePaySmell.MANAGER, 100).Pay(),
            new Ocp.Manager(100).Pay(), 3);
    }

    [Fact]
    public void Lsp_SquareBreaksTheRectangleContract()
    {
        var rect = new LspBad.Rectangle();
        rect.SetWidth(5);
        rect.SetHeight(4);
        Assert.Equal(20, rect.Area());

        var square = new LspBad.Square();
        square.SetWidth(5);
        square.SetHeight(4);           // silently resets width — contract broken
        Assert.NotEqual(20, square.Area());
        Assert.Equal(16, square.Area());
    }

    [Fact]
    public void Lsp_FixedShapesComputeTheirOwnArea()
    {
        Assert.Equal(20.0, new LspGood.Rectangle(5, 4).Area(), 3);
        Assert.Equal(25.0, new LspGood.Square(5).Area(), 3);
    }

    [Fact]
    public void Isp_ClientsDependOnlyOnWhatTheyUse()
    {
        Isp.IPrinter printer = new Isp.BasicPrinter();
        Assert.Equal("printed: cv.pdf", printer.Print("cv.pdf"));
        var aio = new Isp.AllInOne();
        Assert.Equal("scanned", aio.Scan());
        Assert.Equal("faxed: form", aio.Fax("form"));
    }

    [Fact]
    public void Dip_ServiceWorksWithAnyInjectedRepository()
    {
        var service = new Dip.OrderService(new Dip.InMemoryRepository());
        service.Place("order-1");
        service.Place("order-2");
        Assert.Equal(2, service.PlacedCount());
    }
}
