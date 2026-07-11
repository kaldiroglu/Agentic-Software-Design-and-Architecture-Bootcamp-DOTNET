// ◀ Slides: Decks 11 & 12 — Layered vs Hexagonal architecture
// Topics 11 & 12 — Layered vs Hexagonal, on the same order domain.

namespace dev.kaldiroglu.bootcamp.layered.persistence
{
    public interface IOrderRepository
    {
        void Save(string order);
        int Count();
    }

    public sealed class InMemoryOrderRepository : IOrderRepository
    {
        private readonly List<string> _orders = new();
        public void Save(string order) => _orders.Add(order);
        public int Count() => _orders.Count;
    }
}

namespace dev.kaldiroglu.bootcamp.layered.business
{
    using dev.kaldiroglu.bootcamp.layered.persistence;

    /// Business layer depends DOWNWARD on persistence — the layered trade-off.
    public sealed class OrderService(IOrderRepository repository)
    {
        public void Place(string order)
        {
            if (string.IsNullOrWhiteSpace(order))
            {
                throw new ArgumentException("order must not be blank");
            }
            repository.Save(order);
        }

        public int PlacedCount() => repository.Count();
    }
}

namespace dev.kaldiroglu.bootcamp.layered.presentation
{
    using dev.kaldiroglu.bootcamp.layered.business;

    /// Presentation → business → persistence: dependencies point strictly down.
    public sealed class OrderController(OrderService service)
    {
        public string Place(string order)
        {
            try
            {
                service.Place(order);
                return "201 Created";
            }
            catch (ArgumentException)
            {
                return "400 Bad Request";
            }
        }
    }
}

namespace dev.kaldiroglu.bootcamp.hexagonal.domain
{
    /// A PORT owned by the domain — this namespace imports no infrastructure.
    public interface IOrderRepository
    {
        void Save(string order);
        int Count();
    }

    /// Domain logic at the centre; depends only on its own port.
    public sealed class OrderService(IOrderRepository repository)
    {
        public void Place(string order)
        {
            if (string.IsNullOrWhiteSpace(order))
            {
                throw new ArgumentException("order must not be blank");
            }
            repository.Save(order);
        }

        public int PlacedCount() => repository.Count();
    }
}

namespace dev.kaldiroglu.bootcamp.hexagonal.adapter
{
    using dev.kaldiroglu.bootcamp.hexagonal.domain;

    /// A DRIVEN ADAPTER: it imports the domain to implement the domain's port.
    /// The dependency points INWARD — the opposite of the layered version.
    public sealed class InMemoryOrderRepository : IOrderRepository
    {
        private readonly List<string> _orders = new();
        public void Save(string order) => _orders.Add(order);
        public int Count() => _orders.Count;
    }
}
