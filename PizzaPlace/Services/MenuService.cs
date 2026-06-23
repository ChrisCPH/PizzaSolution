using PizzaPlace.Models.Types;

namespace PizzaPlace.Services;

public class MenuService : IMenuService
{
    private static readonly Menu StandardMenu = new("Standard Menu", new ComparableList<MenuItem>
        {
            new("Classic Margherita", PizzaRecipeType.StandardPizza, 89),
            new("Pepperoni Feast", PizzaRecipeType.ExtremelyTastyPizza, 99),
            new("BBQ Chicken", PizzaRecipeType.ExtremelyTastyPizza, 109),
            new("Vegetarian Delight", PizzaRecipeType.StandardPizza, 95),
            new("Spicy Inferno", PizzaRecipeType.OddPizza, 115),
            new("Four Cheese", PizzaRecipeType.StandardPizza, 105),
            new("Mushroom Supreme", PizzaRecipeType.RarePizza, 102),
            new("Hawaiian", PizzaRecipeType.OddPizza, 98),
            new("Horseradish Special", PizzaRecipeType.HorseRadishPizza, 120),
            new("Meat Lovers", PizzaRecipeType.ExtremelyTastyPizza, 125),
            new("Seafood Surprise", PizzaRecipeType.RarePizza, 135),
            new("Mystery Pizza", PizzaRecipeType.EmptyPizza, 80)
        });

    private static readonly Menu LunchMenu = new("Lunch Menu", new ComparableList<MenuItem>
        {
            new("Lunch Margherita", PizzaRecipeType.StandardPizza, 69),
            new("Lunch Pepperoni", PizzaRecipeType.ExtremelyTastyPizza, 75),
            new("Lunch BBQ Chicken", PizzaRecipeType.ExtremelyTastyPizza, 79),
            new("Lunch Vegetarian", PizzaRecipeType.StandardPizza, 72),
            new("Lunch Spicy Inferno", PizzaRecipeType.OddPizza, 82),
            new("Lunch Four Cheese", PizzaRecipeType.StandardPizza, 78),
            new("Lunch Mushroom Supreme", PizzaRecipeType.RarePizza, 76),
            new("Lunch Hawaiian", PizzaRecipeType.OddPizza, 74),
            new("Lunch Horseradish Special", PizzaRecipeType.HorseRadishPizza, 85),
            new("Lunch Meat Lovers", PizzaRecipeType.ExtremelyTastyPizza, 88),
            new("Lunch Seafood Surprise", PizzaRecipeType.RarePizza, 92),
            new("Lunch Mystery Pizza", PizzaRecipeType.EmptyPizza, 65)
        });

    public Menu GetMenu(DateTimeOffset menuDate)
    {
        var hour = menuDate.UtcDateTime.Hour;

        return hour >= 11 && hour < 14
            ? LunchMenu
            : StandardMenu;
    }
}
