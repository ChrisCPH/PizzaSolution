using PizzaPlace.Models;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Services;

public interface IOrderingService
{
    Task<PizzaOrderResult> HandlePizzaOrder(PizzaOrderRequest request);
    Task<OrderStatus> GetOrderStatus(Guid orderId);
}
