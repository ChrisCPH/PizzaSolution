using PizzaPlace.Factories;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Pizzas;
using PizzaPlace.Repositories;

namespace PizzaPlace.Services;

public class OrderingService(
    IStockService stockService,
    IRecipeService recipeService,
    IPizzaOven pizzaOven,
    IOrderRepository orderRepository,
    IMenuService menuService,
    TimeProvider timeProvider) : IOrderingService
{
    private record GroupedOrderItem(PizzaRecipeDto Recipe, int Amount, double Price);

    public async Task<PizzaOrderResult> HandlePizzaOrder(PizzaOrderRequest request)
    {
        var menu = await menuService.GetMenu(timeProvider.GetUtcNow());

        var resolvedItems = request.RequestedOrder
            .Select(requested =>
            {
                var menuItem = menu.Items.FirstOrDefault(mi => mi.Id == requested.MenuItemId)
                    ?? throw new PizzaException($"Menu item {requested.MenuItemId} is not available on the current menu.");

                if (menuItem.SoldOut)
                    throw new PizzaException($"Menu item '{menuItem.Description}' is currently sold out.");

                return (menuItem, requested.Amount);
            })
            .ToList();

        var internalOrder = new PizzaOrder(
            resolvedItems
                .Select(x => new PizzaAmount(x.menuItem.PizzaRecipe.RecipeType, x.Amount))
                .ToComparableList()
        );

        var recipes = await recipeService.GetPizzaRecipes(internalOrder);
        if (await stockService.HasInsufficientStock(internalOrder, recipes))
            throw new PizzaException("Unable to take in order. Insufficient stock.");

        var stock = await stockService.GetStock(internalOrder, recipes);

        var groupedOrder = resolvedItems
            .GroupBy(x => x.menuItem.PizzaRecipe.RecipeType)
            .Select(g =>
            {
                var menuItem = g.First().menuItem;
                var recipe = GetPizzaRecipe(menuItem.PizzaRecipe.RecipeType, recipes);
                var amount = g.Sum(x => (int)x.Amount);
                return new GroupedOrderItem(recipe, amount, menuItem.Price);
            })
            .ToList();

        var prepareOrder = groupedOrder
            .Select(x => new PizzaPrepareOrder(x.Recipe, x.Amount))
            .ToComparableList();

        var cookingTimeMinutes = pizzaOven.CalculateCookingTime(prepareOrder);
        var totalPrice = groupedOrder.Sum(x => x.Price * x.Amount);

        var orderEntity = new Order
        {
            Id = Guid.NewGuid(),
            State = OrderState.Pending,
            CreatedAt = timeProvider.GetUtcNow(),
            LineItems = groupedOrder.Select(x => new OrderLineItem
            {
                PizzaRecipeId = x.Recipe.Id,
                Amount = x.Amount,
                CompletedAmount = 0
            }).ToList()
        };

        await orderRepository.AddOrder(orderEntity);

        var orderId = orderEntity.Id;

        _ = pizzaOven.PreparePizzas(prepareOrder, stock).ContinueWith(async t =>
        {
            if (t.IsFaulted)
            {
                await orderRepository.UpdateOrder(new Order
                {
                    Id = orderId,
                    State = OrderState.Failed,
                    Error = t.Exception!.InnerException!.Message
                });
                return;
            }

            var completedCounts = t.Result
                .GroupBy(p => p.Type)
                .ToDictionary(g => g.Key, g => g.Count());

            var lineItemUpdates = groupedOrder.Select(x => new OrderLineItem
            {
                PizzaRecipeId = x.Recipe.Id,
                CompletedAmount = completedCounts.TryGetValue(x.Recipe.RecipeType, out var count) ? count : 0
            }).ToList();

            await orderRepository.UpdateOrder(new Order
            {
                Id = orderId,
                State = OrderState.Completed,
                LineItems = lineItemUpdates
            });

            Console.WriteLine($"[Order {orderId}] Your pizzas are ready! ({t.Result.Count()} pizza(s) completed)");
        });

        var orderedItems = resolvedItems
            .Select(x => new OrderedItemResult(x.menuItem.Description, x.Amount, x.menuItem.Price))
            .ToComparableList();

        return new PizzaOrderResult(orderId, cookingTimeMinutes, totalPrice, orderedItems);

        PizzaRecipeDto GetPizzaRecipe(PizzaRecipeType pizzaType, ComparableList<PizzaRecipeDto> recipes) =>
            recipes.FirstOrDefault(x => x.RecipeType == pizzaType) ??
            throw new PizzaException($"Missing recipe. Recipe service did not return a recipe for {pizzaType} which was expected.");
    }

    public async Task<OrderStatus> GetOrderStatus(Guid orderId)
    {
        var order = await orderRepository.GetOrder(orderId);

        return new OrderStatus(
            order.State,
            LineItems: order.LineItems.Select(li => new OrderLineItemStatus(
                li.PizzaRecipe.RecipeType,
                li.Amount,
                li.CompletedAmount
            )),
            Error: order.Error
        );
    }
}