namespace PizzaPlace.Models;

public record PizzaOrderRequest(ComparableList<MenuOrderAmount> RequestedOrder);