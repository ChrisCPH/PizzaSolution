using PizzaPlace.DB;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;

namespace PizzaPlace.Test.Repositories;

[TestClass]
public class DBRecipeRepositoryTests
{
    private static IRecipeRepository CreateRepository(AppDbContext context)
        => new RecipeRepository(context);

    private static ComparableList<StockDto> GetIngredients() =>
    [
        new StockDto(StockType.Dough, 1),
        new StockDto(StockType.Tomatoes, 2),
        new StockDto(StockType.GratedCheese, 1),
    ];

    private static PizzaRecipeDto GetStandardRecipe() =>
        new PizzaRecipeDto(PizzaRecipeType.StandardPizza, GetIngredients(), 10);

    private static PizzaRecipeDto GetTastyRecipe() =>
        new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza, GetIngredients(), 15);

    [TestMethod]
    public async Task AddRecipe_ShouldInsertIntoDatabase()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);

        // Act
        var id = await repo.AddRecipe(GetStandardRecipe());

        // Assert
        Assert.IsTrue(id > 0);
        Assert.AreEqual(1, context.PizzaRecipes.Count());
    }

    [TestMethod]
    public async Task AddRecipe_ShouldPersistIngredients()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);

        // Act
        await repo.AddRecipe(GetStandardRecipe());

        // Assert
        var saved = context.PizzaRecipes.First();
        Assert.AreEqual(GetIngredients().Count, context.Stocks.Count());
        Assert.AreEqual(PizzaRecipeType.StandardPizza, saved.RecipeType);
        Assert.AreEqual(10, saved.CookingTimeMinutes);
    }

    [TestMethod]
    public async Task AddRecipe_DuplicateRecipeType_ShouldThrow()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);

        // Act
        await repo.AddRecipe(GetStandardRecipe());
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(
            () => repo.AddRecipe(GetStandardRecipe()));

        // Assert
        Assert.AreEqual($"Recipe already added for {PizzaRecipeType.StandardPizza}.", ex.Message);
    }

    [TestMethod]
    public async Task AddRecipe_TwoDifferentTypes_ShouldBothSucceed()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);

        // Act
        await repo.AddRecipe(GetStandardRecipe());
        await repo.AddRecipe(GetTastyRecipe());

        // Assert
        Assert.AreEqual(2, context.PizzaRecipes.Count());
    }

    [TestMethod]
    public async Task GetRecipe_ShouldReturnCorrectRecipe()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);
        await repo.AddRecipe(GetStandardRecipe());

        // Act
        var result = await repo.GetRecipe(PizzaRecipeType.StandardPizza);

        // Assert
        Assert.AreEqual(PizzaRecipeType.StandardPizza, result.RecipeType);
        Assert.AreEqual(10, result.CookingTimeMinutes);
        Assert.AreEqual(GetIngredients().Count, result.Ingredients.Count);
    }

    [TestMethod]
    public async Task GetRecipe_NonExistentType_ShouldThrow()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(
            () => repo.GetRecipe(PizzaRecipeType.StandardPizza));

        // Assert
        Assert.AreEqual($"Recipe does not exist for {PizzaRecipeType.StandardPizza}.", ex.Message);
    }

    [TestMethod]
    public async Task UpdateRecipe_ShouldUpdateCookingTime()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);
        await repo.AddRecipe(GetStandardRecipe());

        // Act
        var updated = new PizzaRecipeDto(PizzaRecipeType.StandardPizza, GetIngredients(), 20);
        var result = await repo.UpdateRecipe(updated);

        // Assert
        Assert.AreEqual(20, result.CookingTimeMinutes);
        Assert.AreEqual(20, context.PizzaRecipes.First().CookingTimeMinutes);
    }

    [TestMethod]
    public async Task UpdateRecipe_ShouldReplaceIngredients()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);
        await repo.AddRecipe(GetStandardRecipe());

        ComparableList<StockDto> newIngredients =
        [
            new StockDto(StockType.FermentedDough, 1),
        ];
        var updated = new PizzaRecipeDto(PizzaRecipeType.StandardPizza, newIngredients, 10);

        // Act
        var result = await repo.UpdateRecipe(updated);

        // Assert
        Assert.AreEqual(1, result.Ingredients.Count);
        Assert.AreEqual(StockType.FermentedDough, result.Ingredients.First().StockType);
        Assert.AreEqual(1, context.Stocks.Count());
    }

    [TestMethod]
    public async Task UpdateRecipe_NonExistentType_ShouldThrow()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(
            () => repo.UpdateRecipe(GetStandardRecipe()));

        // Assert
        Assert.AreEqual($"Recipe does not exist for {PizzaRecipeType.StandardPizza}.", ex.Message);
    }
}