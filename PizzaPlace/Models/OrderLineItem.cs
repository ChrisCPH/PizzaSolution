namespace PizzaPlace.Models
{
    public class OrderLineItem
    {
        public long Id { get; set; }
        public int Amount { get; set; }
        public int CompletedAmount { get; set; }

        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public long PizzaRecipeId { get; set; }
        public PizzaRecipe PizzaRecipe { get; set; } = null!;
    }
}
