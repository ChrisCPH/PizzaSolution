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
            _pizzaQueue.Enqueue((MakePizza(recipe, cookingTime), guid));
        }
    }

    private Func<Task<Pizza?>> MakePizza(PizzaRecipeDto recipe, int cookingTimeMinutes) => async () =>
    {
        await CookPizza(cookingTimeMinutes);
        return GetPizza(recipe.RecipeType);
    };
}
