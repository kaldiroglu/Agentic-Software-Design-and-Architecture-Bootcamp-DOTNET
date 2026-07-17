// ◀ Slides: Deck 04 SOLID — SRP · OCP · LSP · ISP · DIP
// Topic 04 — SOLID. Each principle as a smell-to-fix pair.

namespace dev.kaldiroglu.bootcamp.solid.srp
{
    /// SMELL — finance + persistence + presentation in one class.
    public sealed class InvoiceSmell(double amount)
    {
        public double Total() => amount;
        public string SaveToDb() => $"INSERT INTO invoices VALUES ({amount})";
        public string ToHtml() => $"<p>Total: {amount}</p>";
    }

    public record Invoice(string Id, double Amount)
    {
        public double Total() => Amount;
    }

    public sealed class InvoiceRepository
    {
        public string Save(Invoice invoice) =>
            $"INSERT INTO invoices VALUES ('{invoice.Id}', {invoice.Amount})";
    }

    public sealed class InvoiceRenderer
    {
        public string ToHtml(Invoice invoice) =>
            $"<p>Invoice {invoice.Id} — Total: {invoice.Total()}</p>";
    }
}

namespace dev.kaldiroglu.bootcamp.solid.ocp
{
    /// SMELL — closed to extension; a new kind edits this method.
    public sealed class PriceCalculatorSmell
    {
        public double Price(string kind, double baseline) => kind switch
        {
            "book" => baseline,
            "food" => baseline * 1.08,
            _ => throw new ArgumentException($"Unknown kind: {kind}")
        };
    }

    /// FIXED — the extension point; a new kind is a new implementation.
    public interface IPricing
    {
        double Price(double baseline);
    }

    public sealed class BookPricing : IPricing
    {
        public double Price(double baseline) => baseline;
    }

    public sealed class FoodPricing : IPricing
    {
        public double Price(double baseline) => baseline * 1.08;
    }

    // ◀ Slides: Deck 04 SOLID — "The Type-Field Smell"

    /// SMELL — an int 'type' field and a method that branches on it; every new
    /// role edits this method (and every other switch on type).
    public sealed class EmployeePaySmell
    {
        public const int ENGINEER = 1;
        public const int SALESMAN = 2;
        public const int MANAGER = 3;

        private readonly int _type;
        private readonly double _base;

        public EmployeePaySmell(int type, double baseline)
        {
            _type = type;
            _base = baseline;
        }

        public double Pay()
        {
            if (_type == ENGINEER) return _base;
            if (_type == SALESMAN) return _base + _base * 0.10;   // + commission
            if (_type == MANAGER) return _base + _base * 0.20;    // + bonus
            throw new InvalidOperationException($"Unknown type: {_type}");
        }
    }

    /// FIXED — each role is a subclass that owns its pay rule; a new role is a
    /// new subclass and this code never changes.
    public abstract class Employee
    {
        protected readonly double Base;

        protected Employee(double baseline) => Base = baseline;

        public abstract double Pay();
    }

    public sealed class Engineer : Employee
    {
        public Engineer(double baseline) : base(baseline) { }
        public override double Pay() => Base;
    }

    public sealed class Salesman : Employee
    {
        public Salesman(double baseline) : base(baseline) { }
        public override double Pay() => Base + Base * 0.10;   // + commission
    }

    public sealed class Manager : Employee
    {
        public Manager(double baseline) : base(baseline) { }
        public override double Pay() => Base + Base * 0.20;   // + bonus
    }
}

namespace dev.kaldiroglu.bootcamp.solid.lsp.violation
{
    public class Rectangle
    {
        protected int Width;
        protected int Height;
        public virtual void SetWidth(int w) => Width = w;
        public virtual void SetHeight(int h) => Height = h;
        public int Area() => Width * Height;
    }

    /// SMELL — to stay square it must break the Rectangle contract.
    public class Square : Rectangle
    {
        public override void SetWidth(int w) { Width = w; Height = w; }
        public override void SetHeight(int h) { Width = h; Height = h; }
    }
}

namespace dev.kaldiroglu.bootcamp.solid.lsp.correct
{
    /// FIXED — no false "is-a"; both are Shapes that report their area.
    public interface IShape
    {
        double Area();
    }

    public record Rectangle(int Width, int Height) : IShape
    {
        public double Area() => (double)Width * Height;
    }

    public record Square(int Side) : IShape
    {
        public double Area() => (double)Side * Side;
    }
}

namespace dev.kaldiroglu.bootcamp.solid.isp
{
    // FIXED — small, focused interfaces; implement only what you support.
    public interface IPrinter { string Print(string doc); }
    public interface IScanner { string Scan(); }
    public interface IFax { string Fax(string doc); }

    public sealed class BasicPrinter : IPrinter
    {
        public string Print(string doc) => $"printed: {doc}";
    }

    public sealed class AllInOne : IPrinter, IScanner, IFax
    {
        public string Print(string doc) => $"printed: {doc}";
        public string Scan() => "scanned";
        public string Fax(string doc) => $"faxed: {doc}";
    }
}

namespace dev.kaldiroglu.bootcamp.solid.dip
{
    /// FIXED — the abstraction high-level policy depends on.
    public interface IRepository
    {
        void Save(string item);
        int Count();
    }

    public sealed class InMemoryRepository : IRepository
    {
        private readonly List<string> _items = new();
        public void Save(string item) => _items.Add(item);
        public int Count() => _items.Count;
    }

    /// SMELL — creates its own concrete detail; can't be tested with a fake.
    public sealed class OrderServiceSmell
    {
        private readonly InMemoryRepository _repo = new();
        public void Place(string order) => _repo.Save(order);
        public int PlacedCount() => _repo.Count();
    }

    /// FIXED — depends on the injected abstraction.
    public sealed class OrderService(IRepository repo)
    {
        public void Place(string order) => repo.Save(order);
        public int PlacedCount() => repo.Count();
    }
}
