using PizzaPlace.Models.Types;

namespace PizzaPlace.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public OrderState State { get; set; }
        public string? Error { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public List<OrderLineItem> LineItems { get; set; } = new();
    }
}
