// ◀ Slides: Deck 05 Design Patterns — Factory · Strategy · Proxy
namespace dev.kaldiroglu.bootcamp.patterns;

// Topic 05 — Factory, Strategy, Proxy.

// ---------- Factory ----------

public interface INotifier
{
    string Send(string message);
}

public sealed class EmailNotifier : INotifier
{
    public string Send(string message) => $"email: {message}";
}

public sealed class SmsNotifier : INotifier
{
    public string Send(string message) => $"sms: {message}";
}

/// The one place that knows the concrete notifiers.
public sealed class NotifierFactory
{
    public enum Channel { Email, Sms }

    public INotifier Create(Channel channel) => channel switch
    {
        Channel.Email => new EmailNotifier(),
        Channel.Sms => new SmsNotifier(),
        _ => throw new ArgumentOutOfRangeException(nameof(channel))
    };
}

// ---------- Strategy ----------

public interface IFee
{
    double Of(double amount);
}

public sealed class CardFee : IFee
{
    public double Of(double amount) => amount * 0.02;
}

public sealed class WireFee : IFee
{
    public double Of(double amount) => 5.0;
}

/// SMELL — the growing if/else; each new kind edits this method.
public sealed class FeeCalculatorSmell
{
    public double Fee(string kind, double amount) => kind switch
    {
        "card" => amount * 0.02,
        "wire" => 5.0,
        _ => throw new ArgumentException($"Unknown kind: {kind}")
    };
}

// ---------- Proxy ----------

public interface IImage
{
    string Render();
}

/// The real, heavyweight object: constructing it "loads from disk".
public sealed class RealImage : IImage
{
    private readonly string _path;

    public RealImage(string path, int[] loads)
    {
        _path = path;
        loads[0]++;   // the expensive work happens on construction
    }

    public string Render() => $"rendered: {_path}";
}

/// A virtual proxy: loads the real image on first render, once.
public sealed class LazyImage(string path, int[] loads) : IImage
{
    private RealImage? _real;

    public string Render()
    {
        _real ??= new RealImage(path, loads);
        return _real.Render();
    }
}
