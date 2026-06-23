using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Models;
using PizzaPlace.Services;

namespace PizzaPlace.Controllers;

[Route("api/order")]
public class OrderingController(
    IOrderingService orderingService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PlacePizzaOrder([FromBody] PizzaOrder pizzaOrder)
    {
        var orderId = await orderingService.HandlePizzaOrder(pizzaOrder);
        return Ok(new
        {
            orderId,
            requestedOrder = pizzaOrder.RequestedOrder
        });
    }

    [HttpGet("{orderId}")]
    public IActionResult GetOrderStatus(Guid orderId)
    {
        var status = orderingService.GetOrderStatus(orderId);

        return Ok(new
        {
            state = status.State.ToString(),
            error = status.Error,
            pizzas = status.Pizzas?
                .GroupBy(p => p.GetType().Name)
                .Select(g => new { pizzaType = g.Key, amount = g.Count() })
        });
    }
}
