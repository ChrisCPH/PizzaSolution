using PizzaPlace.Models.Types;

namespace PizzaPlace.Models
{
    public record OrderStatus(OrderState State, IEnumerable<OrderLineItemStatus>? LineItems = null, string? Error = null);

    public record OrderLineItemStatus(PizzaRecipeType RecipeType, int Amount, int CompletedAmount);
}