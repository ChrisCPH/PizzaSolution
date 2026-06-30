namespace PizzaPlace.Models;

public record PizzaOrderResult(Guid OrderId, int CookingTimeMinutes, double TotalPrice, ComparableList<OrderedItemResult> Order);