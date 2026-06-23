using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Services;

[TestClass]
public class StockServiceTests
{
    private static StockService GetService(Mock<IStockRepository> stockRepository) =>
        new(stockRepository.Object);

    [TestMethod]
    public async Task GetStock()
    {         
        // Arrange
        var order = new PizzaOrder([
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1),
            new PizzaAmount(PizzaRecipeType.ExtremelyTastyPizza, 2),
        ]);

        var standardRecipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
            [
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1),
            ], 10);

        var tastyRecipe = new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza,
            [
                new StockDto(StockType.UnicornDust, 1),
                new StockDto(StockType.BellPeppers, 2),
            ], 15);

        ComparableList<PizzaRecipeDto> recipes = [standardRecipe, tastyRecipe];

        ComparableList<StockDto> expectedStock = [
            new StockDto(StockType.Dough, 100),
            new StockDto(StockType.Tomatoes, 50),
            new StockDto(StockType.UnicornDust, 10),
            new StockDto(StockType.BellPeppers, 20),
        ];

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.Setup(x => x.GetStock(StockType.Dough))
            .ReturnsAsync(new StockDto(StockType.Dough, 100));
        stockRepository.Setup(x => x.GetStock(StockType.Tomatoes))
            .ReturnsAsync(new StockDto(StockType.Tomatoes, 50));
        stockRepository.Setup(x => x.GetStock(StockType.UnicornDust))
            .ReturnsAsync(new StockDto(StockType.UnicornDust, 10));
        stockRepository.Setup(x => x.GetStock(StockType.BellPeppers))
            .ReturnsAsync(new StockDto(StockType.BellPeppers, 20));
        
        var stockService = GetService(stockRepository);

        // Act
        var actual = await stockService.GetStock(order, recipes);

        // Assert
        Assert.AreEqual(expectedStock, actual);
    }

    [TestMethod]
    public async Task HasInsufficientStock()
    {
        // Arrange
        var order = new PizzaOrder([
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1),
            new PizzaAmount(PizzaRecipeType.ExtremelyTastyPizza, 2),
        ]);

        var standardRecipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
            [
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1),
            ], 10);

        var tastyRecipe = new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza,
            [
                new StockDto(StockType.UnicornDust, 1),
                new StockDto(StockType.BellPeppers, 2),
            ], 15);

        ComparableList<PizzaRecipeDto> recipes = [standardRecipe, tastyRecipe];

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);

        stockRepository.Setup(x => x.GetStock(StockType.Dough))
            .ReturnsAsync(new StockDto(StockType.Dough, 1)); // Insufficient stock
        stockRepository.Setup(x => x.GetStock(StockType.Tomatoes))
            .ReturnsAsync(new StockDto(StockType.Tomatoes, 50));
        stockRepository.Setup(x => x.GetStock(StockType.UnicornDust))
            .ReturnsAsync(new StockDto(StockType.UnicornDust, 10));
        stockRepository.Setup(x => x.GetStock(StockType.BellPeppers))
            .ReturnsAsync(new StockDto(StockType.BellPeppers, 20));

        var stockService = GetService(stockRepository);

        // Act
        var actual = await stockService.HasInsufficientStock(order, recipes);

        // Assert
        Assert.IsTrue(actual);
    }
}
