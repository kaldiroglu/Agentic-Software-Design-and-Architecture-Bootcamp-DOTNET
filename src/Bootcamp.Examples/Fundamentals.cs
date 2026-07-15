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

// ---------- Leaky Abstractions — cohesion meets coupling ----------

public sealed record Item(string Name);

/// SMELL — X leaks its internal list via GetItems(); Y and Z reach in and mutate it.
/// Callers are now coupled to X's representation — low cohesion shows up as coupling
/// (plus a Law-of-Demeter train wreck: x.GetItems().Add(...)).
public static class Leaky
{
    public sealed class X
    {
        private readonly List<Item> _items = new();
        public List<Item> GetItems() => _items;   // leaks the internal list
        public int Count => _items.Count;
    }

    public sealed class Y(X x)
    {
        public void Add(Item item) => x.GetItems().Add(item);        // reaches into X
    }

    public sealed class Z(X x)
    {
        public void Remove(Item item) => x.GetItems().Remove(item);  // reaches into X
    }
}

/// FIXED — X owns the behaviour (AddItem/RemoveItem) and keeps the list private. Only
/// the interface is exposed; callers depend on X, never on its representation.
public static class Sealed
{
    public sealed class X
    {
        private readonly List<Item> _items = new();
        public bool AddItem(Item item) { _items.Add(item); return true; }
        public bool RemoveItem(Item item) => _items.Remove(item);
        public int Count => _items.Count;
    }

    public sealed class Y(X x)
    {
        public void Add(Item item) => x.AddItem(item);         // asks X to act
    }

    public sealed class Z(X x)
    {
        public void Remove(Item item) => x.RemoveItem(item);   // asks X to act
    }
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
