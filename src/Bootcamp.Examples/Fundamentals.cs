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
