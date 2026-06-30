using PizzaPlace.Models;

namespace PizzaPlace.Repositories
{
    public interface IOrderRepository
    {
        Task<Guid> AddOrder(Order order);
        Task<Order> GetOrder(Guid orderId);
        Task UpdateOrder(Order order);
    }
}
