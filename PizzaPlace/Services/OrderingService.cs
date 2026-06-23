using PizzaPlace.Factories;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Services;

public class OrderingService(
    IStockService stockService,
    IRecipeService recipeService,
    IPizzaOven pizzaOven) : IOrderingService
{
    private readonly Dictionary<Guid, OrderStatus> _orders = [];

    public async Task<Guid> HandlePizzaOrder(PizzaOrder order)
    {
        var recipes = await recipeService.GetPizzaRecipes(order);
        if (await stockService.HasInsufficientStock(order, recipes))
            throw new PizzaException("Unable to take in order. Insufficient stock.");

        var stock = await stockService.GetStock(order, recipes);

        var prepareOrder = order.RequestedOrder
            .GroupBy(x => x.PizzaType)
            .Select(x => new PizzaPrepareOrder(GetPizzaRecipe(x.Key, recipes), x.Aggregate(0, (total, request) => total + request.Amount)))
            .ToComparableList();

        var orderId = Guid.NewGuid();
        _orders[orderId] = new OrderStatus(OrderState.Pending);

        _ = pizzaOven.PreparePizzas(prepareOrder, stock).ContinueWith(t =>
        {
            _orders[orderId] = t.IsFaulted
                ? new OrderStatus(OrderState.Failed, Error: t.Exception!.InnerException!.Message)
                : new OrderStatus(OrderState.Completed, t.Result);
        });

        return orderId;

        PizzaRecipeDto GetPizzaRecipe(PizzaRecipeType pizzaType, ComparableList<PizzaRecipeDto> recipes) =>
            recipes.FirstOrDefault(x => x.RecipeType == pizzaType) ??
            throw new PizzaException($"Missing recipe. Recipe service did not return a recipe for {pizzaType} which was expected.");
    }

    public OrderStatus GetOrderStatus(Guid orderId) =>
        _orders.TryGetValue(orderId, out var status)
            ? status
            : throw new PizzaException($"Order {orderId} not found.");
}
