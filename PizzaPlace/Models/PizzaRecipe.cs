using PizzaPlace.Models.Types;

namespace PizzaPlace.Models;

public class PizzaRecipe
{
    public long Id { get; set; }

    public int CookingTimeMinutes { get; set; }

    public PizzaRecipeType RecipeType { get; set; }

    public List<Stock> Ingredients { get; set; } = new();
}