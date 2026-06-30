using Microsoft.EntityFrameworkCore;
using PizzaPlace.Models;

namespace PizzaPlace.DB
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<PizzaRecipe> PizzaRecipes { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLineItem> OrderLineItems { get; set; }
    }
}
