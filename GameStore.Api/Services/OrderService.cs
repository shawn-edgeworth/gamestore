using FluentValidation;
using GameStore.Api.Data;
using GameStore.Api.Models;

namespace GameStore.Api.Services
{
    public class OrderService
    {
        private readonly GameRepository gameRepository;
        private readonly OrderRepository orderRepository;
        private readonly IValidator<CreateOrderRequest> createOrderValidator;

        public OrderService(
            GameRepository gameRepository,
            OrderRepository orderRepository,
            IValidator<CreateOrderRequest> createOrderValidator)
        {
            this.createOrderValidator = createOrderValidator;
            this.gameRepository = gameRepository;
            this.orderRepository = orderRepository;
        }

        public async Task<OperationResult<Order>> CreateOrder(int userId, CreateOrderRequest request)
        {
            var validation = await createOrderValidator.ValidateAsync(request);

            if (!validation.IsValid)
            {
                return OperationResult<Order>.Failure(validation.Errors[0].ErrorMessage,
                    OperationErrorType.Validation);
            }

            var orderItems = new List<OrderItem>();

            foreach (var item in request.OrderItems)
            {
                var game = await gameRepository.GetGameById(item.GameId);
                if (game is null)
                {
                    return OperationResult<Order>.Failure("Game is not found.",
                   OperationErrorType.NotFound);
                }

                orderItems.Add(new OrderItem
                {
                    GameId = item.GameId,
                    Quantity = item.Quantity,
                    UnitPrice = game.Price,
                    Game = game
                });
            }

            var order = await orderRepository.CreateOrder(userId, orderItems);

            return OperationResult<Order>.Success(order);
        }

        public async Task<OperationResult<List<Order>>> GetOrders(int userId)
        {
            List<Order> orders = await orderRepository.GetOrders(userId);
            return OperationResult<List<Order>>.Success(orders);
        }
    }
}