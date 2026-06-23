using PizzaPlace.Pizzas;
using PizzaPlace.Models.Types;

namespace PizzaPlace.Models
{
    public record OrderStatus(OrderState State, IEnumerable<Pizza>? Pizzas = null, string? Error = null);
}
