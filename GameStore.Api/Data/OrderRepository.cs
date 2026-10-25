using GameStore.Api.Models;

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
    }
}