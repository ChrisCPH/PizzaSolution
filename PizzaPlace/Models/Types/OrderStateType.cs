namespace PizzaPlace.Models.Types;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderState 
{ 
    Pending, 
    Completed, 
    Failed 
}
