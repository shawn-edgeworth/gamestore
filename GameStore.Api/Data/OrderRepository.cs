using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Data
{
    public class OrderRepository
    {
        private readonly GameDbContext dbContext;

        public OrderRepository(GameDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Order> CreateOrder(int userId, List<OrderItem> orderItems)
        {
            var order = new Order
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Items = orderItems
            };

            dbContext.Orders.Add(order);
            await dbContext.SaveChangesAsync();

            return order;
        }

        public async Task<List<Order>> GetOrders(int userId)
        {
            return await dbContext.Orders
            .Include(x => x.Items)
            .ThenInclude(x => x.Game)
            .Where(x => x.UserId == userId).ToListAsync();
        }
    }
}