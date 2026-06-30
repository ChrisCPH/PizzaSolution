using Microsoft.EntityFrameworkCore;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;

namespace PizzaPlace.DB
{
    public class InventorySeeder
    {
        private readonly AppDbContext _context;

        public InventorySeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.InventoryItems.AnyAsync())
                return;

            var stock = new List<InventoryItem>
            {
                new() { StockType = StockType.Dough, Amount = 100 },
                new() { StockType = StockType.FermentedDough, Amount = 100 },
                new() { StockType = StockType.Tomatoes, Amount = 100 },
                new() { StockType = StockType.RottenTomatoes, Amount = 100 },
                new() { StockType = StockType.UnicornDust, Amount = 100 },
                new() { StockType = StockType.Anchovies, Amount = 100 },
                new() { StockType = StockType.BellPeppers, Amount = 100 },
                new() { StockType = StockType.GratedCheese, Amount = 100 },
                new() { StockType = StockType.UngratedCheese, Amount = 100 },
                new() { StockType = StockType.GenericSpices, Amount = 100 },
                new() { StockType = StockType.UngenericSpices, Amount = 100 },
                new() { StockType = StockType.Sulphur, Amount = 100 },
                new() { StockType = StockType.Bacon, Amount = 100 },
                new() { StockType = StockType.DoubleBacon, Amount = 100 },
                new() { StockType = StockType.TrippleBacon, Amount = 100 },
                new() { StockType = StockType.RayOfSunshine, Amount = 100 },
                new() { StockType = StockType.Chocolate, Amount = 100 },
            };

            _context.InventoryItems.AddRange(stock);
            await _context.SaveChangesAsync();
        }
    }
}