namespace PizzaPlace.Models
{
    public class MenuItem
    {
        public long Id { get; set; }
        public string Description { get; set; } = null!;
        public double Price { get; set; }
        public bool SoldOut { get; set; }

        public long MenuId { get; set; }

        [JsonIgnore]
        public Menu Menu { get; set; } = null!;

        public long PizzaRecipeId { get; set; }
        public PizzaRecipe PizzaRecipe { get; set; } = null!;
    }
}