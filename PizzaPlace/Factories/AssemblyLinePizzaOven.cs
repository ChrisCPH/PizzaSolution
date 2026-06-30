using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Factories;

/// <summary>
/// Producing one line of pizza. 
/// Taking 7 minutes to setup - and then 5 minutes less for every subsequent pizza of the same recipe type to a minimum of 4 minutes.
/// </summary>
public class AssemblyLinePizzaOven(TimeProvider timeProvider) : PizzaOven(timeProvider)
{
    private const int AssemblyLineCapacity = 1;
    public const int SetupTimeMinutes = 7;
    public const int SubsequentPizzaTimeSavingsInMinutes = 5;
    public const int MinimumCookingTimeMinutes = 4;

    protected override int Capacity => AssemblyLineCapacity;

    protected override void PlanPizzaMaking(IEnumerable<(PizzaRecipeDto Recipe, Guid Guid)> recipeOrders)
    {
        var cookingTimeByType = new Dictionary<PizzaRecipeType, int>();

        foreach (var (recipe, guid) in recipeOrders)
        {
            var cookingTime = NextCookingTime(cookingTimeByType, recipe);
            _pizzaQueue.Enqueue((MakePizza(recipe, cookingTime), guid));
        }
    }

    public override int CalculateCookingTime(ComparableList<PizzaPrepareOrder> order)
    {
        var cookingTimeByType = new Dictionary<PizzaRecipeType, int>();
        var total = 0;

        foreach (var prepareOrder in order)
        {
            for (var i = 0; i < prepareOrder.OrderAmount; i++)
            {
                total += NextCookingTime(cookingTimeByType, prepareOrder.RecipeDto);
            }
        }

        return total;
    }

    private static int NextCookingTime(Dictionary<PizzaRecipeType, int> cookingTimeByType, PizzaRecipeDto recipe)
    {
        int cookingTime;
        if (!cookingTimeByType.TryGetValue(recipe.RecipeType, out var previousTime))
        {
            cookingTime = recipe.CookingTimeMinutes + SetupTimeMinutes;
        }
        else
        {
            cookingTime = Math.Max(previousTime - SubsequentPizzaTimeSavingsInMinutes, MinimumCookingTimeMinutes);
        }

        cookingTimeByType[recipe.RecipeType] = cookingTime;
        return cookingTime;
    }

    private Func<Task<Pizza?>> MakePizza(PizzaRecipeDto recipe, int cookingTimeMinutes) => async () =>
    {
        await CookPizza(cookingTimeMinutes);
        return GetPizza(recipe.RecipeType);
    };
}