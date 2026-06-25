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
        var batches = recipeOrders
            .GroupBy(x => x.Recipe.CookingTimeMinutes)
            .SelectMany(g => g.Chunk(GiantRevolvingPizzaOvenCapacity))
            .ToList();

        EnqueueBatch(batches, 0);
    }

    private void EnqueueBatch(List<(PizzaRecipeDto Recipe, Guid Guid)[]> batches, int batchIndex)
    {
        if (batchIndex >= batches.Count) return;

        var batch = batches[batchIndex];
        var batchCount = batch.Length;
        var completedCount = 0;

        foreach (var (recipe, guid) in batch)
        {
            _pizzaQueue.Enqueue((async () =>
            {
                await CookPizza(recipe.CookingTimeMinutes);
                var pizza = GetPizza(recipe.RecipeType);

                if (Interlocked.Increment(ref completedCount) == batchCount)
                    EnqueueBatch(batches, batchIndex + 1);

                return pizza;
            }, guid));
        }
    }
}
