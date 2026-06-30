using PizzaPlace.Factories;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Pizzas;
using PizzaPlace.Repositories;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Services;

[TestClass]
public class OrderingServiceTests
{
    private static OrderingService GetService(
        IStockService stockService,
        IRecipeService recipeService,
        IPizzaOven pizzaOven,
        IOrderRepository orderRepository,
        IMenuService menuService,
        TimeProvider timeProvider) =>
        new OrderingService(stockService, recipeService, pizzaOven, orderRepository, menuService, timeProvider);

    private static PizzaRecipe MakeRecipeEntity(long id, PizzaRecipeType type) =>
        new PizzaRecipe { Id = id, RecipeType = type, CookingTimeMinutes = 10 };

    [TestMethod]
    public async Task HandlePizzaOrder()
    {
        // Arrange
        var time = new DateTimeOffset(2030, 6, 1, 10, 0, 0, TimeSpan.Zero); // outside lunch hours
        var timeProvider = new FakeTimeProvider(time);

        var standardRecipeEntity = MakeRecipeEntity(1, PizzaRecipeType.StandardPizza);
        var tastyRecipeEntity = MakeRecipeEntity(2, PizzaRecipeType.ExtremelyTastyPizza);

        var standardMenuItem = new MenuItem { Id = 10, Description = "Classic Margherita", Price = 89, SoldOut = false, PizzaRecipeId = 1, PizzaRecipe = standardRecipeEntity };
        var tastyMenuItem = new MenuItem { Id = 11, Description = "Pepperoni Feast", Price = 99, SoldOut = false, PizzaRecipeId = 2, PizzaRecipe = tastyRecipeEntity };

        var menu = new Menu { Id = 1, Title = "Standard Menu", Items = [standardMenuItem, tastyMenuItem] };

        var requestedOrder = new ComparableList<MenuOrderAmount>
        {
            new MenuOrderAmount(standardMenuItem.Id, 58),
            new MenuOrderAmount(tastyMenuItem.Id, 2),
        };
        var request = new PizzaOrderRequest(requestedOrder);

        var standardRecipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
            [
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1),
            ], 10, Id: 1);

        var tastyRecipe = new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza,
            [
                new StockDto(StockType.UnicornDust, 1),
                new StockDto(StockType.BellPeppers, 2),
            ], 15, Id: 2);

        ComparableList<PizzaRecipeDto> recipes = [standardRecipe, tastyRecipe];
        ComparableList<StockDto> returnedStock = [new StockDto(StockType.Anchovies, 2)];

        ComparableList<PizzaPrepareOrder> prepareOrders =
        [
            new PizzaPrepareOrder(standardRecipe, 58),
            new PizzaPrepareOrder(tastyRecipe, 2),
        ];

        var pizzas = new List<Pizza>{
            new StandardPizza(),
            new ExtremelyTastyPizza(),
        };

        var internalOrder = new PizzaOrder(
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 58),
            new PizzaAmount(PizzaRecipeType.ExtremelyTastyPizza, 2),
        ]);

        var stockService = new Mock<IStockService>(MockBehavior.Strict);
        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        var pizzaOven = new Mock<IPizzaOven>(MockBehavior.Strict);
        var orderRepository = new Mock<IOrderRepository>(MockBehavior.Strict);
        var menuService = new Mock<IMenuService>(MockBehavior.Strict);

        menuService.Setup(x => x.GetMenu(time))
            .ReturnsAsync(menu);
        recipeService.Setup(x => x.GetPizzaRecipes(It.Is<PizzaOrder>(o =>
                o.RequestedOrder.Any(a => a.PizzaType == PizzaRecipeType.StandardPizza && a.Amount == 58) &&
                o.RequestedOrder.Any(a => a.PizzaType == PizzaRecipeType.ExtremelyTastyPizza && a.Amount == 2))))
            .ReturnsAsync(recipes);
        stockService.Setup(x => x.HasInsufficientStock(It.IsAny<PizzaOrder>(), recipes))
            .ReturnsAsync(false);
        stockService.Setup(x => x.GetStock(It.IsAny<PizzaOrder>(), recipes))
            .ReturnsAsync(returnedStock);
        orderRepository.Setup(x => x.AddOrder(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o.Id);
        pizzaOven.Setup(x => x.CalculateCookingTime(prepareOrders))
            .Returns(10);
        pizzaOven.Setup(x => x.PreparePizzas(prepareOrders, returnedStock))
            .ReturnsAsync(pizzas);

        var service = GetService(stockService.Object, recipeService.Object, pizzaOven.Object, orderRepository.Object, menuService.Object, timeProvider);

        // Act
        var actual = await service.HandlePizzaOrder(request);

        // Assert
        Assert.IsInstanceOfType<PizzaOrderResult>(actual);
        Assert.AreEqual(10, actual.CookingTimeMinutes);
        Assert.AreEqual(58 * 89 + 2 * 99, actual.TotalPrice);
        menuService.VerifyAll();
        stockService.VerifyAll();
        recipeService.VerifyAll();
        orderRepository.Verify(x => x.AddOrder(It.IsAny<Order>()), Times.Once);
        pizzaOven.Verify(x => x.PreparePizzas(prepareOrders, returnedStock), Times.Once);
    }

    [TestMethod]
    public async Task HandlePizzaOrder_MenuItemNotFound()
    {
        // Arrange
        var time = new DateTimeOffset(2030, 6, 1, 10, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(time);
        var menu = new Menu { Id = 1, Title = "Standard Menu", Items = [] };

        var request = new PizzaOrderRequest([new MenuOrderAmount(999, 1)]);

        var stockService = new Mock<IStockService>(MockBehavior.Strict);
        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        var pizzaOven = new Mock<IPizzaOven>(MockBehavior.Strict);
        var orderRepository = new Mock<IOrderRepository>(MockBehavior.Strict);
        var menuService = new Mock<IMenuService>(MockBehavior.Strict);

        menuService.Setup(x => x.GetMenu(time))
            .ReturnsAsync(menu);

        var service = GetService(stockService.Object, recipeService.Object, pizzaOven.Object, orderRepository.Object, menuService.Object, timeProvider);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => service.HandlePizzaOrder(request));

        // Assert
        Assert.AreEqual("Menu item 999 is not available on the current menu.", ex.Message);
        menuService.VerifyAll();
    }

    [TestMethod]
    public async Task HandlePizzaOrder_MenuItemSoldOut()
    {
        // Arrange
        var time = new DateTimeOffset(2030, 6, 1, 10, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(time);
        var recipeEntity = MakeRecipeEntity(1, PizzaRecipeType.StandardPizza);
        var soldOutItem = new MenuItem { Id = 10, Description = "Classic Margherita", Price = 89, SoldOut = true, PizzaRecipeId = 1, PizzaRecipe = recipeEntity };
        var menu = new Menu { Id = 1, Title = "Standard Menu", Items = [soldOutItem] };

        var request = new PizzaOrderRequest([new MenuOrderAmount(10, 1)]);

        var stockService = new Mock<IStockService>(MockBehavior.Strict);
        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        var pizzaOven = new Mock<IPizzaOven>(MockBehavior.Strict);
        var orderRepository = new Mock<IOrderRepository>(MockBehavior.Strict);
        var menuService = new Mock<IMenuService>(MockBehavior.Strict);

        menuService.Setup(x => x.GetMenu(time))
            .ReturnsAsync(menu);

        var service = GetService(stockService.Object, recipeService.Object, pizzaOven.Object, orderRepository.Object, menuService.Object, timeProvider);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => service.HandlePizzaOrder(request));

        // Assert
        Assert.AreEqual("Menu item 'Classic Margherita' is currently sold out.", ex.Message);
        menuService.VerifyAll();
    }

    [TestMethod]
    public async Task HandlePizzaOrder_InsufficientStock()
    {
        // Arrange
        var time = new DateTimeOffset(2030, 6, 1, 10, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(time);

        var tastyRecipeEntity = MakeRecipeEntity(2, PizzaRecipeType.ExtremelyTastyPizza);
        var tastyMenuItem = new MenuItem { Id = 11, Description = "Pepperoni Feast", Price = 99, SoldOut = false, PizzaRecipeId = 2, PizzaRecipe = tastyRecipeEntity };
        var menu = new Menu { Id = 1, Title = "Standard Menu", Items = [tastyMenuItem] };

        var request = new PizzaOrderRequest([new MenuOrderAmount(tastyMenuItem.Id, 2)]);

        var tastyRecipe = new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza,
            [
                new StockDto(StockType.UnicornDust, 1),
                new StockDto(StockType.BellPeppers, 2),
            ], 15, Id: 2);
        ComparableList<PizzaRecipeDto> recipes = [tastyRecipe];

        var stockService = new Mock<IStockService>(MockBehavior.Strict);
        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        var pizzaOven = new Mock<IPizzaOven>(MockBehavior.Strict);
        var orderRepository = new Mock<IOrderRepository>(MockBehavior.Strict);
        var menuService = new Mock<IMenuService>(MockBehavior.Strict);

        menuService.Setup(x => x.GetMenu(time))
            .ReturnsAsync(menu);
        recipeService.Setup(x => x.GetPizzaRecipes(It.IsAny<PizzaOrder>()))
            .ReturnsAsync(recipes);
        stockService.Setup(x => x.HasInsufficientStock(It.IsAny<PizzaOrder>(), recipes))
            .ReturnsAsync(true);

        var service = GetService(stockService.Object, recipeService.Object, pizzaOven.Object, orderRepository.Object, menuService.Object, timeProvider);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => service.HandlePizzaOrder(request));

        // Assert
        Assert.AreEqual("Unable to take in order. Insufficient stock.", ex.Message);
        menuService.VerifyAll();
        stockService.VerifyAll();
        recipeService.VerifyAll();
        pizzaOven.VerifyAll();
    }
}