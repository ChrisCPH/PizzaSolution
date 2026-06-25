using PizzaPlace.Models.Types;

namespace PizzaPlace.Models
{
    public class InventoryItem
    {
        public long Id { get; set; }
        public StockType StockType { get; set; }
        public int Amount { get; set; }
    }
}
