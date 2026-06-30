namespace PizzaPlace.Models
{
    public class Menu
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public List<MenuItem> Items { get; set; } = new();
    }
}
