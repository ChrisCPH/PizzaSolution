using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Models;
using PizzaPlace.Services;

namespace PizzaPlace.Controllers;

[Route("api/order")]
public class OrderingController(
    IOrderingService orderingService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PlacePizzaOrder([FromBody] PizzaOrderRequest pizzaOrderRequest)
    {
        var result = await orderingService.HandlePizzaOrder(pizzaOrderRequest);
        return Ok(result);
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrderStatus(Guid orderId)
    {
        var status = await orderingService.GetOrderStatus(orderId);

        return Ok(new
        {
            state = status.State.ToString(),
            error = status.Error,
            lineItems = status.LineItems?
                .Select(li => new
                {
                    pizzaType = li.RecipeType.ToString(),
                    amount = li.Amount,
                    completedAmount = li.CompletedAmount
                })
        });
    }
}