using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Services;

[TestClass]
public class RecipeServiceTests
{
    private static RecipeService GetService(Mock<IRecipeRepository> recipeRepository) =>
        new(recipeRepository.Object);

    [TestMethod]
    public async Task GetPizzaRecipes()
    {
        // Arrange
        var order = new PizzaOrder([
            new PizzaAmount(PizzaRecipeType.RarePizza, 1),
            new PizzaAmount(PizzaRecipeType.OddPizza, 2),
            new PizzaAmount(PizzaRecipeType.RarePizza, 20),
        ]);
        var rareRecipe = new PizzaRecipeDto(PizzaRecipeType.RarePizza, [new StockDto(StockType.UnicornDust, 1)], 1);
        var oddRecipe = new PizzaRecipeDto(PizzaRecipeType.OddPizza, [new StockDto(StockType.Sulphur, 10)], 100);
        ComparableList<PizzaRecipeDto> expected = [rareRecipe, oddRecipe];

        var recipeRepository = new Mock<IRecipeRepository>(MockBehavior.Strict);
        recipeRepository.Setup(x => x.GetRecipe(PizzaRecipeType.RarePizza))
            .ReturnsAsync(rareRecipe);
        recipeRepository.Setup(x => x.GetRecipe(PizzaRecipeType.OddPizza))
            .ReturnsAsync(oddRecipe);

        var service = GetService(recipeRepository);

        // Act
        var actual = await service.GetPizzaRecipes(order);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public async Task AddRecipe()
    {
        // Arrange
        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
            [new StockDto(StockType.Dough, 1)], 10);

        var recipeRepository = new Mock<IRecipeRepository>(MockBehavior.Strict);
        recipeRepository.Setup(x => x.AddRecipe(recipe))
            .ReturnsAsync(1L);

        var service = GetService(recipeRepository);

        // Act
        var actual = await service.AddRecipe(recipe);

        // Assert
        Assert.AreEqual(recipe, actual);
        recipeRepository.VerifyAll();
    }

    [TestMethod]
    public async Task AddRecipe_AlreadyExists_Throws()
    {
        // Arrange
        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
            [new StockDto(StockType.Dough, 1)], 10);

        var recipeRepository = new Mock<IRecipeRepository>(MockBehavior.Strict);
        recipeRepository.Setup(x => x.AddRecipe(recipe))
            .ThrowsAsync(new PizzaException($"Recipe already added for {recipe.RecipeType}."));

        var service = GetService(recipeRepository);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<PizzaException>(() => service.AddRecipe(recipe));
        recipeRepository.VerifyAll();
    }

    [TestMethod]
    public async Task UpdateRecipe()
    {
        // Arrange
        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
            [new StockDto(StockType.Dough, 2)], 15);

        var recipeRepository = new Mock<IRecipeRepository>(MockBehavior.Strict);
        recipeRepository.Setup(x => x.UpdateRecipe(recipe))
            .ReturnsAsync(recipe);

        var service = GetService(recipeRepository);

        // Act
        var actual = await service.UpdateRecipe(recipe);

        // Assert
        Assert.AreEqual(recipe, actual);
        recipeRepository.VerifyAll();
    }

    [TestMethod]
    public async Task UpdateRecipe_DoesNotExist_Throws()
    {
        // Arrange
        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
            [new StockDto(StockType.Dough, 2)], 15);

        var recipeRepository = new Mock<IRecipeRepository>(MockBehavior.Strict);
        recipeRepository.Setup(x => x.UpdateRecipe(recipe))
            .ThrowsAsync(new PizzaException($"Recipe does not exist for {recipe.RecipeType}."));

        var service = GetService(recipeRepository);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<PizzaException>(() => service.UpdateRecipe(recipe));
        recipeRepository.VerifyAll();
    }
}
