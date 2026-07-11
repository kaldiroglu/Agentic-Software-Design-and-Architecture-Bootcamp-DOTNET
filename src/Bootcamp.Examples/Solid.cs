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
