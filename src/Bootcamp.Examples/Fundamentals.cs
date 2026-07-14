// ◀ Slides: Deck 02 Fundamentals — cohesion, coupling, Law of Demeter
namespace dev.kaldiroglu.bootcamp.fundamentals;

// Topic 02 — Cohesion, Coupling and the Law of Demeter.

// ---------- Cohesion ----------

/// SMELL — a low-cohesion "God utility": four unrelated jobs in one class.
public sealed class UtilsSmell
{
    private readonly Dictionary<string, string> _users = new();

    public void SaveUser(string id, string name) => _users[id] = name;       // persistence
    public string? LoadUser(string id) => _users.GetValueOrDefault(id);       // persistence
    public string SendEmail(string to, string body) => $"TO:{to}\n{body}";    // messaging
    public double CalcTax(double amount) => amount * 0.20;                     // finance
    public string FormatIsoDate(int y, int m, int d) => $"{y:D4}-{m:D2}-{d:D2}"; // formatting
}

/// FIXED — one job: storing and retrieving users.
public sealed class UserRepository
{
    private readonly Dictionary<string, string> _users = new();
    public void Save(string id, string name) => _users[id] = name;
    public string? FindName(string id) => _users.GetValueOrDefault(id);
}

/// FIXED — one job: computing tax.
public sealed class TaxCalculator(double rate)
{
    public double TaxOn(double amount) => amount * rate;
}

/// FIXED — one job: composing an e-mail.
public sealed class EmailComposer
{
    public string Compose(string to, string subject, string body) =>
        $"To: {to}\nSubject: {subject}\n\n{body}";
}

/// FIXED — a highly cohesive value object: validation, random creation, comparison
/// and masking for a password all live here, and nothing unrelated leaks in.
public sealed class Password
{
    private const int MinLength = 12;
    private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Lower = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string Symbols = "!@#$%^&*()-_=+";
    private static readonly Random Rng = new();   // demo only; use RandomNumberGenerator in production
    private readonly string _value;

    private Password(string value) => _value = value;

    // create & validate — one place owns the rules
    public static Password Of(string raw) =>
        IsValid(raw) ? new Password(raw)
                     : throw new ArgumentException("Password does not meet the policy");

    public static bool IsValid(string? raw) =>
        raw is not null
        && raw.Length >= MinLength
        && raw.Any(char.IsUpper)
        && raw.Any(char.IsLower)
        && raw.Any(char.IsDigit)
        && raw.Any(ch => !char.IsLetterOrDigit(ch));

    // generate a strong random password (reuses Of()/IsValid)
    public static Password Random(int length)
    {
        if (length < MinLength) throw new ArgumentException($"length must be at least {MinLength}");
        var chars = new List<char> { Pick(Upper), Pick(Lower), Pick(Digits), Pick(Symbols) };
        var pool = Upper + Lower + Digits + Symbols;
        while (chars.Count < length) chars.Add(Pick(pool));
        for (var i = chars.Count - 1; i > 0; i--)   // shuffle
        {
            var j = Rng.Next(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
        return Of(new string(chars.ToArray()));
    }

    // compare safely — constant-time, no early exit on first difference
    public bool Matches(string? candidate)
    {
        if (candidate is null || candidate.Length != _value.Length) return false;
        var diff = 0;
        for (var i = 0; i < _value.Length; i++) diff |= _value[i] ^ candidate[i];
        return diff == 0;
    }

    // format & mask — never leak the raw value
    public string Masked() => new('•', _value.Length);

    public string MaskedShowingLast(int shown)
    {
        if (shown <= 0) return Masked();
        var hidden = Math.Max(0, _value.Length - shown);
        return new string('•', hidden) + _value[hidden..];
    }

    public override string ToString() => Masked();   // safe by default
    public override bool Equals(object? other) => other is Password p && Matches(p._value);
    public override int GetHashCode() => _value.GetHashCode();

    private static char Pick(string pool) => pool[Rng.Next(pool.Length)];
}

// ---------- Coupling ----------

/// FIXED — the abstraction the report depends on.
public interface IDatabase
{
    IReadOnlyList<string> Query(string sql);
}

/// FIXED — a lightweight database for tests and demos.
public sealed class InMemoryDatabase(IReadOnlyList<string> rows) : IDatabase
{
    public IReadOnlyList<string> Query(string sql) => rows;
}

/// SMELL — the report creates its own concrete data source; welded to it.
public sealed class ReportSmell
{
    private readonly MySqlStub _db = new();
    public int RowCount() => _db.Query("SELECT * FROM sales").Count;

    private sealed class MySqlStub
    {
        public IReadOnlyList<string> Query(string sql) => new[] { "hard-wired-row" };
    }
}

/// FIXED — depends on the IDatabase abstraction, injected from outside.
public sealed class Report(IDatabase db)
{
    public int RowCount() => db.Query("SELECT * FROM sales").Count;
}

// ---------- OOP Coupling — Subtyping & Message (DIP) ----------

/// FIXED — the abstraction collaborators depend on. "Program to an interface,
/// not an implementation"; depend on abstractions, not concretions (DIP).
public interface IMailer
{
    void Send(string receipt);
}

/// A tiny order that knows how to describe its own receipt.
public record MailOrder(string Id)
{
    public string Receipt() => $"Receipt for order {Id}";
}

/// FIXED — message coupling, the loosest bond: Orders talks to IMailer only
/// through messages and shares no state. The mailer is injected, so any mailer
/// swaps in with no change here (DIP).
public sealed class Orders(IMailer mailer)
{
    // Only a message crosses the boundary — nothing else is shared.
    public void Place(MailOrder order) => mailer.Send(order.Receipt());
}

/// SMELL — "program to an implementation": creates its own concrete mailer with
/// new, welding itself to it. No seam to swap it or to intercept the mail.
public sealed class OrdersSmell
{
    private readonly SmtpMailer _mailer = new();
    public void Place(MailOrder order) => _mailer.Send(order.Receipt());

    private sealed class SmtpMailer
    {
        public void Send(string receipt) { /* pretend a real socket opens */ }
    }
}

/// FIXED — an IMailer test double; the seam that message coupling buys us.
public sealed class RecordingMailer : IMailer
{
    private readonly List<string> _sent = new();
    public void Send(string receipt) => _sent.Add(receipt);
    public IReadOnlyList<string> Sent => _sent;
}

/// A simple base ledger; its Entries list is an internal implementation detail.
public class Ledger
{
    protected readonly List<int> Entries = new();
    public void Add(int amount) => Entries.Add(amount);
    public int Total() => Entries.Sum();
}

/// SMELL — subtyping coupling: reaches into the parent's protected Entries,
/// binding to Ledger's internal shape. Change the storage and this breaks.
public sealed class AuditLedgerSmell : Ledger
{
    public int AuditTotal()
    {
        var sum = 0;
        foreach (var amount in Entries) sum += amount;   // welded to parent data
        return sum;
    }
}

/// FIXED — composition over inheritance: holds a Ledger and uses only its public API.
public sealed class AuditLedger(Ledger ledger)
{
    public int AuditTotal() => ledger.Total();   // depends only on the public surface
}

// ---------- Law of Demeter ----------

public record Address(string Street, string City);

public record DemeterCustomer(string Name, Address Address)
{
    public string ShippingCity() => Address.City;   // tell-don't-ask
}

public record DemeterOrder(string Id, DemeterCustomer Customer)
{
    /// FIXED — the order hides its structure; callers depend on Order alone.
    public string ShippingCity() => Customer.ShippingCity();
}

/// SMELL — the "train wreck": reaches through Order → Customer → Address.
public sealed class ShippingLabelSmell
{
    public string CityFor(DemeterOrder order) => order.Customer.Address.City;
}
