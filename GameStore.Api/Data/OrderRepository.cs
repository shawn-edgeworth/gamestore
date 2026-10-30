using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Data
{
    public class OrderRepository
    {
        private readonly GameDbContext dbContext;

        private IQueryable<Order> OrderWithItems =>
            dbContext.Orders
            .Include(x => x.Items)
            .ThenInclude(x => x.Game);

        public OrderRepository(GameDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Order> CreateOrder(User user, decimal spentMoney, List<OrderItem> orderItems)
        {
            var order = new Order
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Items = orderItems,
                OrderStatus = OrderStatus.Paid
            };

            user.Balance -= spentMoney;
            dbContext.Orders.Add(order);
            await dbContext.SaveChangesAsync();

            return order;
        }

        public async Task<List<Order>> GetOrders(int userId)
        {
            return await OrderWithItems
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<Order?> GetOrder(int orderId)
        {
            var order = await OrderWithItems
                .FirstOrDefaultAsync(x => x.Id == orderId);
            return order;
        }

        internal async Task<Order> Checkout(User user, Cart cart, decimal spentMoney, List<OrderItem> orderItems)
        {
            var order = new Order
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Items = orderItems,
                OrderStatus = OrderStatus.Paid
            };

            user.Balance -= spentMoney;
            cart.Items.Clear();
            dbContext.Orders.Add(order);
            await dbContext.SaveChangesAsync();

            return order;
        }

        public async Task<Order> Refund(User user, decimal totalPrice, Order order)
        {
            user.Balance += totalPrice;
            order.OrderStatus = OrderStatus.Refunded;
            await dbContext.SaveChangesAsync();

            return order;
        }
    }
}