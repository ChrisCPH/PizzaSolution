using PizzaPlace.Models;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Services;

public interface IOrderingService
{
    Task<Guid> HandlePizzaOrder(PizzaOrder order);
    OrderStatus GetOrderStatus(Guid orderId);
}
