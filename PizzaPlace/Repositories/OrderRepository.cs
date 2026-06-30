using Microsoft.EntityFrameworkCore;
using PizzaPlace.DB;
using PizzaPlace.Models;

namespace PizzaPlace.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> AddOrder(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order.Id;
        }

        public async Task<Order> GetOrder(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.LineItems)
                    .ThenInclude(li => li.PizzaRecipe)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new PizzaException($"Order {orderId} not found.");

            return order;
        }

        public async Task UpdateOrder(Order order)
        {
            var existing = await _context.Orders
                .Include(o => o.LineItems)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            if (existing == null)
                throw new PizzaException($"Order {order.Id} not found.");

            existing.State = order.State;
            existing.Error = order.Error;

            foreach (var lineItem in existing.LineItems)
            {
                var updated = order.LineItems.FirstOrDefault(li => li.PizzaRecipeId == lineItem.PizzaRecipeId);
                if (updated != null)
                    lineItem.CompletedAmount = updated.CompletedAmount;
            }

            await _context.SaveChangesAsync();
        }
    }
}