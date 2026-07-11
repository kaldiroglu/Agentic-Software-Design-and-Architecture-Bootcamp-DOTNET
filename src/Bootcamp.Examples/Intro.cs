// ◀ Slides: Deck 01 Introduction — order-total example
namespace dev.kaldiroglu.bootcamp.intro;

// Topic 01 — Introduction. Order total: tangled (smell) vs decision-separated (fixed).

public record Item(string Name, double Price, int Quantity)
{
    public double LineTotal() => Price * Quantity;
}

public record Customer(string Name, bool Vip);

public record Order(IReadOnlyList<Item> Items, Customer Customer)
{
    public double Subtotal() => Items.Sum(i => i.LineTotal());
}

/// SMELL — one method knows the subtotal, the discount rule AND the tax rule.
public sealed class OrderTotalSmell
{
    public double Total(Order order)
    {
        double total = 0;
        foreach (var item in order.Items)
        {
            total += item.Price * item.Quantity;
        }
        if (order.Customer.Vip)   // discount rule
        {
            total *= 0.9;
        }
        total += total * 0.20;    // tax rule
        return total;
    }
}

/// FIXED — the discount decision pulled out behind a small boundary.
public interface IDiscount
{
    double Apply(double subtotal, Customer customer);
}

public sealed class NoDiscount : IDiscount
{
    public double Apply(double subtotal, Customer customer) => subtotal;
}

public sealed class VipDiscount : IDiscount
{
    public double Apply(double subtotal, Customer customer) =>
        customer.Vip ? subtotal * 0.9 : subtotal;
}

/// FIXED — totalling does one thing: sum, apply whichever discount, add tax.
public sealed class OrderTotal(IDiscount discount)
{
    private const double TaxRate = 0.20;

    public double Total(Order order) =>
        discount.Apply(order.Subtotal(), order.Customer) * (1 + TaxRate);
}
