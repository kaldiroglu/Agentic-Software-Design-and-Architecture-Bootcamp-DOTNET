using dev.kaldiroglu.bootcamp.intro;
using Xunit;

namespace Bootcamp.Examples.Tests;

public class IntroTests
{
    private static Order VipOrder() =>
        new([new Item("book", 50.0, 2)], new Customer("Ada", true));

    private static Order PlainOrder() =>
        new([new Item("book", 50.0, 2)], new Customer("Kamil", false));

    [Fact]
    public void SmellAndFixedAgreeForVip()
    {
        var smell = new OrderTotalSmell().Total(VipOrder());
        var fixedTotal = new OrderTotal(new VipDiscount()).Total(VipOrder());
        Assert.Equal(108.0, fixedTotal, 3);
        Assert.Equal(smell, fixedTotal, 3);
    }

    [Fact]
    public void DiscountRuleIsSwappable()
    {
        var withVip = new OrderTotal(new VipDiscount()).Total(PlainOrder());
        var withNone = new OrderTotal(new NoDiscount()).Total(PlainOrder());
        Assert.Equal(120.0, withVip, 3);
        Assert.Equal(120.0, withNone, 3);
    }
}
