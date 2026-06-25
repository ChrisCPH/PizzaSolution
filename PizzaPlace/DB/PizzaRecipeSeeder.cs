namespace PizzaPlace.DB
{
    using Microsoft.EntityFrameworkCore;
    using PizzaPlace.Models;
    using PizzaPlace.Models.Types;

    public class PizzaRecipeSeeder
    {
        private readonly AppDbContext _context;

        public PizzaRecipeSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.PizzaRecipes.AnyAsync())
                return;

            var recipes = new List<PizzaRecipe>
        {
            new PizzaRecipe
            {
                RecipeType = PizzaRecipeType.StandardPizza,
                CookingTimeMinutes = 10,
                Ingredients = new List<Stock>
                {
                    new() { StockType = StockType.Dough, Amount = 1 },
                    new() { StockType = StockType.Tomatoes, Amount = 2 },
                    new() { StockType = StockType.GratedCheese, Amount = 1 },
                    new() { StockType = StockType.GenericSpices, Amount = 1 }
                }
            },

            new PizzaRecipe
            {
                RecipeType = PizzaRecipeType.ExtremelyTastyPizza,
                CookingTimeMinutes = 10,
                Ingredients = new List<Stock>
                {
                    new() { StockType = StockType.FermentedDough, Amount = 1 },
                    new() { StockType = StockType.Tomatoes, Amount = 1 },
                    new() { StockType = StockType.RottenTomatoes, Amount = 1 },
                    new() { StockType = StockType.GratedCheese, Amount = 1 },
                    new() { StockType = StockType.UngenericSpices, Amount = 1 }
                }
            },

            new PizzaRecipe
            {
                RecipeType = PizzaRecipeType.OddPizza,
                CookingTimeMinutes = 12,
                Ingredients = new List<Stock>
                {
                    new() { StockType = StockType.Dough, Amount = 1 },
                    new() { StockType = StockType.RottenTomatoes, Amount = 2 },
                    new() { StockType = StockType.Anchovies, Amount = 1 },
                    new() { StockType = StockType.UngratedCheese, Amount = 1 },
                    new() { StockType = StockType.Sulphur, Amount = 1 }
                }
            },

            new PizzaRecipe
            {
                RecipeType = PizzaRecipeType.RarePizza,
                CookingTimeMinutes = 20,
                Ingredients = new List<Stock>
                {
                    new() { StockType = StockType.FermentedDough, Amount = 1 },
                    new() { StockType = StockType.UnicornDust, Amount = 1 },
                    new() { StockType = StockType.RayOfSunshine, Amount = 1 },
                    new() { StockType = StockType.Chocolate, Amount = 1 },
                    new() { StockType = StockType.GratedCheese, Amount = 1 }
                }
            },

            new PizzaRecipe
            {
                RecipeType = PizzaRecipeType.HorseRadishPizza,
                CookingTimeMinutes = 11,
                Ingredients = new List<Stock>
                {
                    new() { StockType = StockType.Dough, Amount = 1 },
                    new() { StockType = StockType.BellPeppers, Amount = 1 },
                    new() { StockType = StockType.GenericSpices, Amount = 1 },
                    new() { StockType = StockType.Bacon, Amount = 1 },
                    new() { StockType = StockType.GratedCheese, Amount = 1 }
                }
            },

            new PizzaRecipe
            {
                RecipeType = PizzaRecipeType.EmptyPizza,
                CookingTimeMinutes = 5,
                Ingredients = new List<Stock>
                {
                    new() { StockType = StockType.Dough, Amount = 1 }
                }
            }
        };

            _context.PizzaRecipes.AddRange(recipes);
            await _context.SaveChangesAsync();
        }
    }
}
