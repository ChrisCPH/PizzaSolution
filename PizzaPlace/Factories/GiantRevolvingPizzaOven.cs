using PizzaPlace.Models;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Factories;

/// <summary>
/// Produces pizza on one giant revolving surface. Downside is, that all pizzas must have the same cooking time, when prepared at the same time.
/// </summary>
public class GiantRevolvingPizzaOven(TimeProvider timeProvider) : PizzaOven(timeProvider)
{
    private const int GiantRevolvingPizzaOvenCapacity = 120;

    protected override int Capacity => GiantRevolvingPizzaOvenCapacity;

    protected override void PlanPizzaMaking(IEnumerable<(PizzaRecipeDto Recipe, Guid Guid)> recipeOrders)
    {
        var orders = recipeOrders.ToList();
        var maxCookingTime = orders.Max(x => x.Recipe.CookingTimeMinutes);

        foreach (var (recipe, guid) in orders)
        {
            _pizzaQueue.Enqueue((MakePizza(recipe, maxCookingTime), guid));
        }
    }
    private Func<Task<Pizza?>> MakePizza(PizzaRecipeDto recipe, int cookingTimeMinutes) => async () =>
    {
        await CookPizza(cookingTimeMinutes);
        return GetPizza(recipe.RecipeType);
    };
}
