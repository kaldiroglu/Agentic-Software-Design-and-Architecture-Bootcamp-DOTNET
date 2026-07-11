using dev.kaldiroglu.bootcamp.patterns;
using Xunit;

namespace Bootcamp.Examples.Tests;

public class PatternsTests
{
    [Fact]
    public void Factory_HidesConcreteTypes()
    {
        var factory = new NotifierFactory();
        Assert.Equal("email: hi", factory.Create(NotifierFactory.Channel.Email).Send("hi"));
        Assert.Equal("sms: hi", factory.Create(NotifierFactory.Channel.Sms).Send("hi"));
    }

    [Fact]
    public void Strategy_MatchesTheTangledVersion()
    {
        var smell = new FeeCalculatorSmell();
        Assert.Equal(smell.Fee("card", 100), new CardFee().Of(100), 3);
        Assert.Equal(smell.Fee("wire", 100), new WireFee().Of(100), 3);
    }

    [Fact]
    public void Proxy_DefersTheLoadUntilFirstRender()
    {
        var loads = new int[1];

        _ = new RealImage("photo.png", loads);   // eager
        Assert.Equal(1, loads[0]);

        IImage lazy = new LazyImage("big.png", loads);
        Assert.Equal(1, loads[0]);               // constructing the proxy loads nothing

        Assert.Equal("rendered: big.png", lazy.Render());
        Assert.Equal(2, loads[0]);               // first render triggers the load

        lazy.Render();
        Assert.Equal(2, loads[0]);               // second render reuses it
    }
}
