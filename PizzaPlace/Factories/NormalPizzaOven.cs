using PizzaPlace.Models;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Factories;

/// <summary>
/// A normal oven producing each pizza one at a time. Each pizza taking the normal cooking time.
/// </summary>
public class NormalPizzaOven(TimeProvider timeProvider) : PizzaOven(timeProvider)
{
    private const int NormalPizzaOvenCapacity = 4;

    protected override int Capacity => NormalPizzaOvenCapacity;

    protected override void PlanPizzaMaking(IEnumerable<(PizzaRecipeDto Recipe, Guid Guid)> recipeOrders)
    {
        foreach (var (recipe, orderGuid) in recipeOrders)
        {
            _pizzaQueue.Enqueue((MakePizza(recipe), orderGuid));
        }
    }

    public override int CalculateCookingTime(ComparableList<PizzaPrepareOrder> order)
    {
        var durations = order
            .SelectMany(o => Enumerable.Repeat(o.RecipeDto.CookingTimeMinutes, o.OrderAmount))
            .OrderBy(d => d)
            .ToList();

        var slotFreeAt = new int[NormalPizzaOvenCapacity];

        foreach (var duration in durations)
        {
            var earliestSlot = Array.IndexOf(slotFreeAt, slotFreeAt.Min());
            slotFreeAt[earliestSlot] += duration;
        }

        return slotFreeAt.Max();
    }

    private Func<Task<Pizza?>> MakePizza(PizzaRecipeDto recipe) => async () =>
    {
        await CookPizza(recipe.CookingTimeMinutes);
        return GetPizza(recipe.RecipeType);
    };
}
