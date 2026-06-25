using PizzaPlace.DB;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;

namespace PizzaPlace.Test.Repositories;

[TestClass]
public class DBStockRepositoryTests
{
    private static IStockRepository CreateRepository(AppDbContext context)
        => new StockRepository(context);


    [TestMethod]
    public async Task AddToStock_ShouldInsertNewStock()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);

        // Act
        var result = await repo.AddToStock(new StockDto(StockType.Dough, 5));

        // Assert
        Assert.AreEqual(StockType.Dough, result.StockType);
        Assert.AreEqual(5, result.Amount);
        Assert.AreEqual(1, context.InventoryItems.Count());
    }

    [TestMethod]
    public async Task AddToStock_ExistingStockType_ShouldAccumulate()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);

        // Act
        await repo.AddToStock(new StockDto(StockType.Dough, 5));
        var result = await repo.AddToStock(new StockDto(StockType.Dough, 3));

        // Assert
        Assert.AreEqual(8, result.Amount);
        Assert.AreEqual(1, context.InventoryItems.Count());
    }

    [TestMethod]
    public async Task AddToStock_NegativeAmount_ShouldThrow()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(
            () => repo.AddToStock(new StockDto(StockType.Dough, -1)));

        // Assert
        Assert.AreEqual("Stock cannot have negative amount.", ex.Message);
    }

    [TestMethod]
    public async Task AddToStock_ZeroAmount_ShouldInsert()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);

        // Act
        var result = await repo.AddToStock(new StockDto(StockType.Dough, 0));

        // Assert
        Assert.AreEqual(0, result.Amount);
        Assert.AreEqual(1, context.InventoryItems.Count());
    }

    [TestMethod]
    public async Task AddToStock_DifferentTypes_ShouldInsertSeparately()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);

        // Act
        await repo.AddToStock(new StockDto(StockType.Dough, 5));
        await repo.AddToStock(new StockDto(StockType.Tomatoes, 3));

        // Assert
        Assert.AreEqual(2, context.InventoryItems.Count());
    }

    [TestMethod]
    public async Task GetStock_ExistingType_ShouldReturnStock()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);
        await repo.AddToStock(new StockDto(StockType.Dough, 5));

        // Act
        var result = await repo.GetStock(StockType.Dough);

        // Assert
        Assert.AreEqual(StockType.Dough, result.StockType);
        Assert.AreEqual(5, result.Amount);
    }

    [TestMethod]
    public async Task GetStock_NonExistentType_ShouldReturnZero()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);

        // Act
        var result = await repo.GetStock(StockType.Dough);

        // Assert
        Assert.AreEqual(StockType.Dough, result.StockType);
        Assert.AreEqual(0, result.Amount);
    }

    [TestMethod]
    public async Task TakeStock_ShouldReduceStock()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);
        await repo.AddToStock(new StockDto(StockType.Dough, 10));

        // Act
        var result = await repo.TakeStock(StockType.Dough, 4);

        // Assert
        Assert.AreEqual(StockType.Dough, result.StockType);
        Assert.AreEqual(4, result.Amount);
        Assert.AreEqual(6, context.InventoryItems.First().Amount);
    }

    [TestMethod]
    public async Task TakeStock_ExactAmount_ShouldReduceToZero()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);
        await repo.AddToStock(new StockDto(StockType.Dough, 5));

        // Act
        var result = await repo.TakeStock(StockType.Dough, 5);

        // Assert
        Assert.AreEqual(5, result.Amount);
        Assert.AreEqual(0, context.InventoryItems.First().Amount);
    }

    [TestMethod]
    public async Task TakeStock_InsufficientStock_ShouldThrow()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);
        await repo.AddToStock(new StockDto(StockType.Dough, 3));

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(
            () => repo.TakeStock(StockType.Dough, 5));

        // Assert
        Assert.AreEqual("Not enough stock to take the given amount.", ex.Message);
    }

    [TestMethod]
    public async Task TakeStock_NonExistentType_ShouldThrow()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(
            () => repo.TakeStock(StockType.Dough, 1));

        // Assert
        Assert.AreEqual("Not enough stock to take the given amount.", ex.Message);
    }

    [TestMethod]
    public async Task TakeStock_ZeroAmount_ShouldThrow()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);
        await repo.AddToStock(new StockDto(StockType.Dough, 5));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
            () => repo.TakeStock(StockType.Dough, 0));
    }

    [TestMethod]
    public async Task TakeStock_NegativeAmount_ShouldThrow()
    {
        // Arrange
        using var context = TestDbFactory.Create();
        var repo = CreateRepository(context);
        await repo.AddToStock(new StockDto(StockType.Dough, 5));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
            () => repo.TakeStock(StockType.Dough, -1));
    }
}