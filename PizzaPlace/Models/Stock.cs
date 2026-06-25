using PizzaPlace.Models.Types;

namespace PizzaPlace.Models;

public class Stock
{
    public long Id { get; set; }

    public StockType StockType { get; set; }

    public int Amount { get; set; }

    public long? PizzaRecipeId { get; set; }

    public PizzaRecipe? PizzaRecipe { get; set; } = null!;
}