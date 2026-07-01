using Moq;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;
using PizzaPlace.Services;
using PizzaPlace.Test.Factories;

namespace PizzaPlace.Test.Services;

[TestClass]
public class MenuServiceTests
{
    private static MenuService GetService(
        Mock<IMenuRepository> menuRepository,
        Mock<IRecipeRepository> recipeRepository,
        Mock<IStockRepository> stockRepository) =>
        new(menuRepository.Object, recipeRepository.Object, stockRepository.Object);

    private static Mock<IMenuRepository> StrictMenuRepo() => new(MockBehavior.Strict);
    private static Mock<IRecipeRepository> StrictRecipeRepo() => new(MockBehavior.Strict);
    private static Mock<IStockRepository> StrictStockRepo() => new(MockBehavior.Strict);

    private static PizzaRecipeDto GetStandardRecipe() =>
        NormalPizzaOvenTests.GetTestStandardPizzaRecipe();

    [TestMethod]
    public async Task GetMenu_ReturnsStandardMenuOutsideLunchHours()
    {
        // Arrange
        var standardMenuTime = new DateTimeOffset(2024, 6, 1, 10, 0, 0, TimeSpan.Zero);
        var standardMenu = new Menu { Id = 1, Title = "Standard Menu", Items = [] };
        var lunchMenu = new Menu { Id = 2, Title = "Lunch Menu", Items = [] };

        var menuRepository = StrictMenuRepo();
        menuRepository.Setup(x => x.GetAllMenus())
            .ReturnsAsync([standardMenu, lunchMenu]);

        var service = GetService(menuRepository, StrictRecipeRepo(), StrictStockRepo());

        // Act
        var menu = await service.GetMenu(standardMenuTime);

        // Assert
        Assert.AreEqual("Standard Menu", menu.Title);
        menuRepository.VerifyAll();
    }

    [TestMethod]
    public async Task GetMenu_ReturnsLunchMenuDuringLunchHours()
    {
        // Arrange
        var lunchTime = new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero);
        var standardMenu = new Menu { Id = 1, Title = "Standard Menu", Items = [] };
        var lunchMenu = new Menu { Id = 2, Title = "Lunch Menu", Items = [] };

        var menuRepository = StrictMenuRepo();
        menuRepository.Setup(x => x.GetAllMenus())
            .ReturnsAsync([standardMenu, lunchMenu]);

        var service = GetService(menuRepository, StrictRecipeRepo(), StrictStockRepo());

        // Act
        var menu = await service.GetMenu(lunchTime);

        // Assert
        Assert.AreEqual("Lunch Menu", menu.Title);
        menuRepository.VerifyAll();
    }

    [TestMethod]
    public async Task GetMenu_MenuNotFound_ShouldThrow()
    {
        // Arrange
        var time = new DateTimeOffset(2024, 6, 1, 10, 0, 0, TimeSpan.Zero);

        var menuRepository = StrictMenuRepo();
        menuRepository.Setup(x => x.GetAllMenus())
            .ReturnsAsync([]);

        var service = GetService(menuRepository, StrictRecipeRepo(), StrictStockRepo());

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => service.GetMenu(time));

        // Assert
        Assert.AreEqual("Menu 'Standard Menu' not found.", ex.Message);
        menuRepository.VerifyAll();
    }

    [TestMethod]
    public async Task AddMenu_ShouldInsertMenu()
    {
        // Arrange
        var menu = new Menu { Title = "New Menu", Items = [] };

        var menuRepository = StrictMenuRepo();
        menuRepository.Setup(x => x.GetAllMenus())
            .ReturnsAsync([]);
        menuRepository.Setup(x => x.AddMenu(menu))
            .ReturnsAsync(1);

        var service = GetService(menuRepository, StrictRecipeRepo(), StrictStockRepo());

        // Act
        var id = await service.AddMenu(menu);

        // Assert
        Assert.AreEqual(1, id);
        menuRepository.VerifyAll();
    }

    [TestMethod]
    public async Task AddMenu_DuplicateTitle_ShouldThrow()
    {
        // Arrange
        var existing = new Menu { Id = 1, Title = "Standard Menu", Items = [] };
        var menu = new Menu { Title = "Standard Menu", Items = [] };

        var menuRepository = StrictMenuRepo();
        menuRepository.Setup(x => x.GetAllMenus())
            .ReturnsAsync([existing]);

        var service = GetService(menuRepository, StrictRecipeRepo(), StrictStockRepo());

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => service.AddMenu(menu));

        // Assert
        Assert.AreEqual("A menu with the title 'Standard Menu' already exists.", ex.Message);
        menuRepository.VerifyAll();
    }

    [TestMethod]
    public async Task UpdateMenu_ShouldUpdateTitle()
    {
        // Arrange
        var menu = new Menu { Id = 1, Title = "Updated Menu", Items = [] };

        var menuRepository = StrictMenuRepo();
        menuRepository.Setup(x => x.GetAllMenus())
            .ReturnsAsync([new Menu { Id = 1, Title = "Old Menu", Items = [] }]);
        menuRepository.Setup(x => x.UpdateMenu(menu))
            .ReturnsAsync(menu);

        var service = GetService(menuRepository, StrictRecipeRepo(), StrictStockRepo());

        // Act
        var result = await service.UpdateMenu(menu);

        // Assert
        Assert.AreEqual("Updated Menu", result.Title);
        menuRepository.VerifyAll();
    }

    [TestMethod]
    public async Task UpdateMenu_DuplicateTitle_ShouldThrow()
    {
        // Arrange
        var menu = new Menu { Id = 2, Title = "Standard Menu", Items = [] };
        var existing = new Menu { Id = 1, Title = "Standard Menu", Items = [] };

        var menuRepository = StrictMenuRepo();
        menuRepository.Setup(x => x.GetAllMenus())
            .ReturnsAsync([existing]);

        var service = GetService(menuRepository, StrictRecipeRepo(), StrictStockRepo());

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => service.UpdateMenu(menu));

        // Assert
        Assert.AreEqual("A menu with the title 'Standard Menu' already exists.", ex.Message);
        menuRepository.VerifyAll();
    }

    [TestMethod]
    public async Task UpdateMenu_SameMenuSameTitle_ShouldNotThrow()
    {
        // Arrange
        var menu = new Menu { Id = 1, Title = "Standard Menu", Items = [] };

        var menuRepository = StrictMenuRepo();
        menuRepository.Setup(x => x.GetAllMenus())
            .ReturnsAsync([new Menu { Id = 1, Title = "Standard Menu", Items = [] }]);
        menuRepository.Setup(x => x.UpdateMenu(menu))
            .ReturnsAsync(menu);

        var service = GetService(menuRepository, StrictRecipeRepo(), StrictStockRepo());

        // Act
        var result = await service.UpdateMenu(menu);

        // Assert
        Assert.AreEqual("Standard Menu", result.Title);
        menuRepository.VerifyAll();
    }

    [TestMethod]
    public async Task AddMenuItem_ShouldInsertItem()
    {
        // Arrange
        var recipe = GetStandardRecipe() with { Id = 1 };
        var item = new MenuItem { Description = "Margherita", Price = 89, MenuId = 1, PizzaRecipeId = 1 };

        var menuRepository = StrictMenuRepo();
        var recipeRepository = StrictRecipeRepo();

        recipeRepository.Setup(x => x.GetRecipeById(1))
            .ReturnsAsync(recipe);
        menuRepository.Setup(x => x.AddMenuItem(item))
            .ReturnsAsync(1);

        var service = GetService(menuRepository, recipeRepository, StrictStockRepo());

        // Act
        var id = await service.AddMenuItem(item);

        // Assert
        Assert.AreEqual(1, id);
        menuRepository.VerifyAll();
        recipeRepository.VerifyAll();
    }

    [TestMethod]
    public async Task AddMenuItem_InvalidRecipe_ShouldThrow()
    {
        // Arrange
        var item = new MenuItem { Description = "Ghost Pizza", Price = 89, MenuId = 1, PizzaRecipeId = 999 };

        var menuRepository = StrictMenuRepo();
        var recipeRepository = StrictRecipeRepo();

        recipeRepository.Setup(x => x.GetRecipeById(999))
            .ThrowsAsync(new PizzaException("Recipe with id 999 does not exist."));

        var service = GetService(menuRepository, recipeRepository, StrictStockRepo());

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => service.AddMenuItem(item));

        // Assert
        Assert.AreEqual("Recipe with id 999 does not exist.", ex.Message);
        recipeRepository.VerifyAll();
    }

    [TestMethod]
    public async Task UpdateMenuItem_ShouldUpdateFields()
    {
        // Arrange
        var recipe = GetStandardRecipe() with { Id = 1 };
        var existing = new MenuItem { Id = 10, Description = "Old Name", Price = 89, SoldOut = false, PizzaRecipeId = 1 };
        var updated = new MenuItem { Id = 10, Description = "New Name", Price = 99, SoldOut = false, PizzaRecipeId = 1 };

        var menuRepository = StrictMenuRepo();
        var recipeRepository = StrictRecipeRepo();

        recipeRepository.Setup(x => x.GetRecipeById(1))
            .ReturnsAsync(recipe);
        menuRepository.Setup(x => x.GetMenuItemById(10))
            .ReturnsAsync(existing);
        menuRepository.Setup(x => x.UpdateMenuItem(updated))
            .ReturnsAsync(updated);

        var service = GetService(menuRepository, recipeRepository, StrictStockRepo());

        // Act
        var result = await service.UpdateMenuItem(updated);

        // Assert
        Assert.AreEqual("New Name", result.Description);
        Assert.AreEqual(99, result.Price);
        menuRepository.VerifyAll();
        recipeRepository.VerifyAll();
    }

    [TestMethod]
    public async Task UpdateMenuItem_UnSoldOut_WithSufficientStock_ShouldSucceed()
    {
        // Arrange
        var recipe = GetStandardRecipe() with { Id = 1 };
        var existing = new MenuItem { Id = 10, Description = "Margherita", Price = 89, SoldOut = true, PizzaRecipeId = 1 };
        var updated = new MenuItem { Id = 10, Description = "Margherita", Price = 89, SoldOut = false, PizzaRecipeId = 1 };

        var menuRepository = StrictMenuRepo();
        var recipeRepository = StrictRecipeRepo();
        var stockRepository = StrictStockRepo();

        recipeRepository.Setup(x => x.GetRecipeById(1))
            .ReturnsAsync(recipe);
        menuRepository.Setup(x => x.GetMenuItemById(10))
            .ReturnsAsync(existing);

        foreach (var ingredient in recipe.Ingredients)
        {
            stockRepository.Setup(x => x.GetStock(ingredient.StockType))
                .ReturnsAsync(new StockDto(ingredient.StockType, ingredient.Amount));
        }

        menuRepository.Setup(x => x.UpdateMenuItem(updated))
            .ReturnsAsync(updated);

        var service = GetService(menuRepository, recipeRepository, stockRepository);

        // Act
        var result = await service.UpdateMenuItem(updated);

        // Assert
        Assert.IsFalse(result.SoldOut);
        menuRepository.VerifyAll();
        recipeRepository.VerifyAll();
        stockRepository.VerifyAll();
    }

    [TestMethod]
    public async Task UpdateMenuItem_UnSoldOut_InsufficientStock_ShouldThrow()
    {
        // Arrange
        var recipe = GetStandardRecipe() with { Id = 1 };
        var existing = new MenuItem { Id = 10, Description = "Margherita", Price = 89, SoldOut = true, PizzaRecipeId = 1 };
        var updated = new MenuItem { Id = 10, Description = "Margherita", Price = 89, SoldOut = false, PizzaRecipeId = 1 };

        var menuRepository = StrictMenuRepo();
        var recipeRepository = StrictRecipeRepo();
        var stockRepository = StrictStockRepo();

        recipeRepository.Setup(x => x.GetRecipeById(1))
            .ReturnsAsync(recipe);
        menuRepository.Setup(x => x.GetMenuItemById(10))
            .ReturnsAsync(existing);

        var firstIngredient = recipe.Ingredients.First();
        stockRepository.Setup(x => x.GetStock(firstIngredient.StockType))
            .ReturnsAsync(new StockDto(firstIngredient.StockType, 0));

        var service = GetService(menuRepository, recipeRepository, stockRepository);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => service.UpdateMenuItem(updated));

        // Assert
        Assert.AreEqual($"Insufficient stock to un-sold-out this item. Not enough {firstIngredient.StockType}.", ex.Message);
        menuRepository.VerifyAll();
        recipeRepository.VerifyAll();
        stockRepository.VerifyAll();
    }

    [TestMethod]
    public async Task UpdateMenuItem_AlreadySoldOut_RemainsTrue_ShouldNotCheckStock()
    {
        // Arrange
        var recipe = GetStandardRecipe() with { Id = 1 };
        var existing = new MenuItem { Id = 10, Description = "Margherita", Price = 89, SoldOut = true, PizzaRecipeId = 1 };
        var updated = new MenuItem { Id = 10, Description = "Margherita", Price = 89, SoldOut = true, PizzaRecipeId = 1 };

        var menuRepository = StrictMenuRepo();
        var recipeRepository = StrictRecipeRepo();

        recipeRepository.Setup(x => x.GetRecipeById(1))
            .ReturnsAsync(recipe);
        menuRepository.Setup(x => x.GetMenuItemById(10))
            .ReturnsAsync(existing);
        menuRepository.Setup(x => x.UpdateMenuItem(updated))
            .ReturnsAsync(updated);

        // StrictStockRepo with no setups — if stock is checked, the test fails
        var service = GetService(menuRepository, recipeRepository, StrictStockRepo());

        // Act
        var result = await service.UpdateMenuItem(updated);

        // Assert
        Assert.IsTrue(result.SoldOut);
        menuRepository.VerifyAll();
        recipeRepository.VerifyAll();
    }
}