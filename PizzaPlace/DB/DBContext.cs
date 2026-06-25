using Microsoft.EntityFrameworkCore;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;

namespace PizzaPlace.DB
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<PizzaRecipe> PizzaRecipes { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
    }
}
