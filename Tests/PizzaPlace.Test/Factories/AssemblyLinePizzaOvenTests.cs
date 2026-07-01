using PizzaPlace.Factories;
using PizzaPlace.Models;
using PizzaPlace.Pizzas;
using PizzaPlace.Test.TestExtensions;

namespace PizzaPlace.Test.Factories;

[TestClass]
public class AssemblyLinePizzaOvenTests
{
    private static AssemblyLinePizzaOven GetOven(TimeProvider timeProvider) => new(timeProvider);

    private static PizzaRecipeDto GetStandardRecipe() =>
        NormalPizzaOvenTests.GetTestStandardPizzaRecipe();

    private static PizzaRecipeDto GetTastyRecipe() =>
        NormalPizzaOvenTests.GetTestTastyPizzaRecipe();

    [TestMethod]
    public async Task PreparePizzas_OnePizza()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var order = new ComparableList<PizzaPrepareOrder>
         {
             new PizzaPrepareOrder(NormalPizzaOvenTests.GetTestStandardPizzaRecipe(), 1),
         };
        var stock = NormalPizzaOvenTests.GetPlentyStock();

        var oven = GetOven(timeProvider);
        var expectedTime = NormalPizzaOvenTests.StandardPizzaPrepareTime + AssemblyLinePizzaOven.SetupTimeMinutes;
        var expectedPizzas = 1;

        // Act
        var pizzasTask = oven.PreparePizzas(order, stock);
        timeProvider.PassTimeInMinuteIntervals(expectedTime - 1);
        var firstCheck = pizzasTask.IsCompleted;
        timeProvider.PassTimeInMinuteIntervals(1);
        var secondCheck = pizzasTask.IsCompleted;

        // Assert
        Assert.IsFalse(firstCheck);
        Assert.IsTrue(secondCheck);
        var pizzas = await pizzasTask;
        Assert.AreEqual(expectedPizzas, pizzas.Count());
        Assert.IsTrue(pizzas.All(x => x is StandardPizza), "Only standard pizzas");
    }

    [TestMethod]
    public async Task PreparePizzas_23_OfTheSameTypePizza()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        ComparableList<PizzaPrepareOrder> order =
        [
            new PizzaPrepareOrder(NormalPizzaOvenTests.GetTestTastyPizzaRecipe(), 23),
         ];
        var stock = NormalPizzaOvenTests.GetPlentyStock();

        var oven = GetOven(timeProvider);
        // NormalPizzaOvenTests.TastyPizzaPrepareTime + AssemblyLinePizzaOven.SetupTimeMinutes = 22
        // AssemblyLinePizzaOven.SubsequentPizzaTimeSavingsInMinutes = 5
        // AssemblyLinePizzaOven.MinimumCookingTimeMinutes = 4
        var expectedTime = new[] {22, 17, 12, 7, 4,
             4, 4, 4, 4, 4,
             4, 4, 4, 4, 4,
             4, 4, 4, 4, 4,
             4, 4, 4}.Sum();
        var expectedPizzas = 23;

        // Act
        var pizzasTask = oven.PreparePizzas(order, stock);
        timeProvider.PassTimeInMinuteIntervals(expectedTime - 1);
        var firstCheck = pizzasTask.IsCompleted;
        timeProvider.PassTimeInMinuteIntervals(1);
        var secondCheck = pizzasTask.IsCompleted;

        // Assert
        Assert.IsFalse(firstCheck);
        Assert.IsTrue(secondCheck);
        var pizzas = await pizzasTask;
        Assert.AreEqual(expectedPizzas, pizzas.Count());
        Assert.IsTrue(pizzas.All(x => x is ExtremelyTastyPizza), "Only tasty pizzas");
    }

    [TestMethod]
    public async Task PreparePizzas_MixedPizzaTypes()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        ComparableList<PizzaPrepareOrder> order =
        [
            new PizzaPrepareOrder(NormalPizzaOvenTests.GetTestTastyPizzaRecipe(), 10),
             new PizzaPrepareOrder(NormalPizzaOvenTests.GetTestStandardPizzaRecipe(), 7),
         ];
        var stock = NormalPizzaOvenTests.GetPlentyStock();

        var oven = GetOven(timeProvider);
        var expectedTime = new[] {22, 17, 12, 7, 4,
             4, 4, 4, 4, 4,
             17, 12, 7, 4, 4,
             4, 4, }.Sum();
        var expectedPizzas = 17;

        // Act
        var pizzasTask = oven.PreparePizzas(order, stock);
        timeProvider.PassTimeInMinuteIntervals(expectedTime - 1);
        var firstCheck = pizzasTask.IsCompleted;
        timeProvider.PassTimeInMinuteIntervals(1);
        var secondCheck = pizzasTask.IsCompleted;

        // Assert
        Assert.IsFalse(firstCheck);
        Assert.IsTrue(secondCheck);
        var pizzas = await pizzasTask;
        Assert.AreEqual(expectedPizzas, pizzas.Count());
        Assert.AreEqual(10, pizzas.Count(x => x is ExtremelyTastyPizza));
        Assert.AreEqual(7, pizzas.Count(x => x is StandardPizza));
    }

    [TestMethod]
    public void CalculateCookingTime_OnePizza_ShouldIncludeSetupTime()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        ComparableList<PizzaPrepareOrder> order = [new(GetStandardRecipe(), 1)];
        var expected = NormalPizzaOvenTests.StandardPizzaPrepareTime + AssemblyLinePizzaOven.SetupTimeMinutes;

        // Act
        var result = GetOven(timeProvider).CalculateCookingTime(order);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void CalculateCookingTime_TwoPizzasSameType_ShouldReduceSecondByDiscount()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        ComparableList<PizzaPrepareOrder> order = [new(GetStandardRecipe(), 2)];
        var first = NormalPizzaOvenTests.StandardPizzaPrepareTime + AssemblyLinePizzaOven.SetupTimeMinutes;
        var second = first - AssemblyLinePizzaOven.SubsequentPizzaTimeSavingsInMinutes;
        var expected = first + second;

        // Act
        var result = GetOven(timeProvider).CalculateCookingTime(order);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void CalculateCookingTime_ManySameType_ShouldNotGoBelowMinimum()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        ComparableList<PizzaPrepareOrder> order = [new(GetStandardRecipe(), 10)];
        var oven = GetOven(timeProvider);

        // Act
        var result = oven.CalculateCookingTime(order);

        // Assert
        // Time per pizza floors at MinimumCookingTimeMinutes, so total can't go below
        // first pizza time + (9 * MinimumCookingTimeMinutes)
        var minimumPossible = NormalPizzaOvenTests.StandardPizzaPrepareTime + AssemblyLinePizzaOven.SetupTimeMinutes
            + 9 * AssemblyLinePizzaOven.MinimumCookingTimeMinutes;
        Assert.IsTrue(result >= minimumPossible);
    }

    [TestMethod]
    public void CalculateCookingTime_TwoDifferentTypes_EachGetsOwnSetupTime()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        ComparableList<PizzaPrepareOrder> order =
        [
            new(GetStandardRecipe(), 1),
            new(GetTastyRecipe(), 1),
        ];
        var expected =
            (NormalPizzaOvenTests.StandardPizzaPrepareTime + AssemblyLinePizzaOven.SetupTimeMinutes) +
            (NormalPizzaOvenTests.TastyPizzaPrepareTime + AssemblyLinePizzaOven.SetupTimeMinutes);

        // Act
        var result = GetOven(timeProvider).CalculateCookingTime(order);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void CalculateCookingTime_EmptyOrder_ShouldReturnZero()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        ComparableList<PizzaPrepareOrder> order = [];

        // Act
        var result = GetOven(timeProvider).CalculateCookingTime(order);

        // Assert
        Assert.AreEqual(0, result);
    }
}
