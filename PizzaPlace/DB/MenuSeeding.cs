using Microsoft.EntityFrameworkCore;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;

namespace PizzaPlace.DB
{
    public class MenuSeeder
    {
        private readonly AppDbContext _context;

        public MenuSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.Menus.AnyAsync())
                return;

            var recipeIds = await _context.PizzaRecipes
                .ToDictionaryAsync(r => r.RecipeType, r => r.Id);

            long RecipeId(PizzaRecipeType type) =>
                recipeIds.TryGetValue(type, out var id)
                    ? id
                    : throw new PizzaException($"Cannot seed menu: recipe {type} not found. Ensure PizzaRecipeSeeder runs first.");

            var standardMenu = new Menu
            {
                Title = "Standard Menu",
                Items = new List<MenuItem>
                {
                    new() { Description = "Classic Margherita", PizzaRecipeId = RecipeId(PizzaRecipeType.StandardPizza), Price = 89 },
                    new() { Description = "Pepperoni Feast", PizzaRecipeId = RecipeId(PizzaRecipeType.ExtremelyTastyPizza), Price = 99 },
                    new() { Description = "BBQ Chicken", PizzaRecipeId = RecipeId(PizzaRecipeType.ExtremelyTastyPizza), Price = 109 },
                    new() { Description = "Vegetarian Delight", PizzaRecipeId = RecipeId(PizzaRecipeType.StandardPizza), Price = 95 },
                    new() { Description = "Spicy Inferno", PizzaRecipeId = RecipeId(PizzaRecipeType.OddPizza), Price = 115 },
                    new() { Description = "Four Cheese", PizzaRecipeId = RecipeId(PizzaRecipeType.StandardPizza), Price = 105 },
                    new() { Description = "Mushroom Supreme", PizzaRecipeId = RecipeId(PizzaRecipeType.RarePizza), Price = 102 },
                    new() { Description = "Hawaiian", PizzaRecipeId = RecipeId(PizzaRecipeType.OddPizza), Price = 98 },
                    new() { Description = "Horseradish Special", PizzaRecipeId = RecipeId(PizzaRecipeType.HorseRadishPizza), Price = 120 },
                    new() { Description = "Meat Lovers", PizzaRecipeId = RecipeId(PizzaRecipeType.ExtremelyTastyPizza), Price = 125 },
                    new() { Description = "Seafood Surprise", PizzaRecipeId = RecipeId(PizzaRecipeType.RarePizza), Price = 135 },
                    new() { Description = "Mystery Pizza", PizzaRecipeId = RecipeId(PizzaRecipeType.EmptyPizza), Price = 80 },
                }
            };

            var lunchMenu = new Menu
            {
                Title = "Lunch Menu",
                Items = new List<MenuItem>
                {
                    new() { Description = "Lunch Margherita", PizzaRecipeId = RecipeId(PizzaRecipeType.StandardPizza), Price = 69 },
                    new() { Description = "Lunch Pepperoni", PizzaRecipeId = RecipeId(PizzaRecipeType.ExtremelyTastyPizza), Price = 75 },
                    new() { Description = "Lunch BBQ Chicken", PizzaRecipeId = RecipeId(PizzaRecipeType.ExtremelyTastyPizza), Price = 79 },
                    new() { Description = "Lunch Vegetarian", PizzaRecipeId = RecipeId(PizzaRecipeType.StandardPizza), Price = 72 },
                    new() { Description = "Lunch Spicy Inferno", PizzaRecipeId = RecipeId(PizzaRecipeType.OddPizza), Price = 82 },
                    new() { Description = "Lunch Four Cheese", PizzaRecipeId = RecipeId(PizzaRecipeType.StandardPizza), Price = 78 },
                    new() { Description = "Lunch Mushroom Supreme", PizzaRecipeId = RecipeId(PizzaRecipeType.RarePizza), Price = 76 },
                    new() { Description = "Lunch Hawaiian", PizzaRecipeId = RecipeId(PizzaRecipeType.OddPizza), Price = 74 },
                    new() { Description = "Lunch Horseradish Special", PizzaRecipeId = RecipeId(PizzaRecipeType.HorseRadishPizza), Price = 85 },
                    new() { Description = "Lunch Meat Lovers", PizzaRecipeId = RecipeId(PizzaRecipeType.ExtremelyTastyPizza), Price = 88 },
                    new() { Description = "Lunch Seafood Surprise", PizzaRecipeId = RecipeId(PizzaRecipeType.RarePizza), Price = 92 },
                    new() { Description = "Lunch Mystery Pizza", PizzaRecipeId = RecipeId(PizzaRecipeType.EmptyPizza), Price = 65 },
                }
            };

            _context.Menus.AddRange(standardMenu, lunchMenu);
            await _context.SaveChangesAsync();
        }
    }
}