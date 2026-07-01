using PizzaPlace.Models;
using PizzaPlace.Models.Types;

namespace PizzaPlace.Repositories;

public class FakeRecipeRepository : FakeDatabase<PizzaRecipeDto>, IRecipeRepository
{
    private static readonly object _lock = new();

    public Task<long> AddRecipe(PizzaRecipeDto recipe)
    {
        lock (_lock)
        {
            if (Get(x => x.RecipeType == recipe.RecipeType).Any())
                throw new PizzaException($"Recipe already added for {recipe.RecipeType}.");

            var id = Insert(recipe);
            return Task.FromResult(id);
        }
    }

    public Task<PizzaRecipeDto> GetRecipe(PizzaRecipeType recipeType)
    {
        var recipe = Get(x => x.RecipeType == recipeType)
            .FirstOrDefault() ?? throw new PizzaException($"Recipe does not exists of type {recipeType}.");

        return Task.FromResult(recipe);
    }

    public Task<PizzaRecipeDto> UpdateRecipe(PizzaRecipeDto recipe)
    {
        lock (_lock)
        {
            var existing = Get(x => x.RecipeType == recipe.RecipeType)
                .FirstOrDefault() ?? throw new PizzaException($"Recipe does not exist for {recipe.RecipeType}.");

            Update(recipe with { Id = existing.Id }, existing.Id);
            return Task.FromResult(recipe);
        }
    }

    public void AddStandardRecipes()
    {
        if (Get(_ => true).Any())
            return;

        foreach (var recipe in GetStandardRecipes())
            AddRecipe(recipe).GetAwaiter().GetResult();

        static List<PizzaRecipeDto> GetStandardRecipes() =>
        [
            new PizzaRecipeDto(PizzaRecipeType.StandardPizza, [
                new StockDto(StockType.Dough, 1),
                new StockDto(StockType.Tomatoes, 2),
                new StockDto(StockType.GratedCheese, 1),
                new StockDto(StockType.GenericSpices, 1)
            ], 10),

            new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza, [
                new StockDto(StockType.FermentedDough, 1),
                new StockDto(StockType.Tomatoes, 1),
                new StockDto(StockType.RottenTomatoes, 1),
                new StockDto(StockType.GratedCheese, 1),
                new StockDto(StockType.UngenericSpices, 1)
            ], 10),

            new PizzaRecipeDto(PizzaRecipeType.OddPizza, [
                new StockDto(StockType.Dough, 1),
                new StockDto(StockType.RottenTomatoes, 2),
                new StockDto(StockType.Anchovies, 1),
                new StockDto(StockType.UngratedCheese, 1),
                new StockDto(StockType.Sulphur, 1)
            ], 12),

            new PizzaRecipeDto(PizzaRecipeType.RarePizza, [
                new StockDto(StockType.FermentedDough, 1),
                new StockDto(StockType.UnicornDust, 1),
                new StockDto(StockType.RayOfSunshine, 1),
                new StockDto(StockType.Chocolate, 1),
                new StockDto(StockType.GratedCheese, 1)
            ], 20),

            new PizzaRecipeDto(PizzaRecipeType.HorseRadishPizza, [
                new StockDto(StockType.Dough, 1),
                new StockDto(StockType.BellPeppers, 1),
                new StockDto(StockType.GenericSpices, 1),
                new StockDto(StockType.Bacon, 1),
                new StockDto(StockType.GratedCheese, 1)
            ], 11),

            new PizzaRecipeDto(PizzaRecipeType.EmptyPizza, [
                new StockDto(StockType.Dough, 1)
            ], 5),
        ];
    }

    public Task<PizzaRecipeDto> GetRecipeById(long id)
    {
        throw new NotImplementedException();
    }
}
