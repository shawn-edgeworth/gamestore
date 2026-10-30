using FluentValidation;
using GameStore.Api.Data;
using GameStore.Api.Models;

namespace GameStore.Api.Services
{
    public class OrderService
    {
        private readonly GameRepository gameRepository;
        private readonly OrderRepository orderRepository;
        private readonly UserRepository userRepository;
        private readonly CartRepository cartRepository;
        private readonly IValidator<CreateOrderRequest> createOrderValidator;

        public OrderService(
            GameRepository gameRepository,
            OrderRepository orderRepository,
            UserRepository userRepository,
            CartRepository cartRepository,
            IValidator<CreateOrderRequest> createOrderValidator)
        {
            this.createOrderValidator = createOrderValidator;
            this.gameRepository = gameRepository;
            this.orderRepository = orderRepository;
            this.userRepository = userRepository;
            this.cartRepository = cartRepository;
        }

        public async Task<OperationResult<Order>> CreateOrder(int userId, CreateOrderRequest request)
        {
            var validation = await createOrderValidator.ValidateAsync(request);

            if (!validation.IsValid)
            {
                return OperationResult<Order>.Failure(validation.Errors[0].ErrorMessage,
                    OperationErrorType.Validation);
            }

            var user = await userRepository.GetUserById(userId);
            if (user is null)
            {
                return OperationResult<Order>.Failure("User is not found.",
                   OperationErrorType.NotFound);
            }

            var orderItems = new List<OrderItem>();
            var totalPrice = 0m;
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
                    GameName = game.Name,
                    Game = game
                });

                totalPrice += item.Quantity * game.Price;
            }

            if (user.Balance < totalPrice)
            {
                return OperationResult<Order>.Failure("User doesn't have enough money.",
                   OperationErrorType.BadRequest);
            }

            var order = await orderRepository.CreateOrder(user, totalPrice, orderItems);
            return OperationResult<Order>.Success(order);
        }

        public async Task<OperationResult<List<Order>>> GetOrders(int userId)
        {
            List<Order> orders = await orderRepository.GetOrders(userId);
            return OperationResult<List<Order>>.Success(orders);
        }

        public async Task<OperationResult<Order>> GetOrder(int orderId, int userId)
        {
            var order = await orderRepository.GetOrder(orderId);
            if (order is null)
            {
                return OperationResult<Order>.Failure("Order is not found.",
                    OperationErrorType.NotFound);
            }

            if (order.UserId != userId)
            {
                return OperationResult<Order>.Failure("No access to that order from this user.",
                    OperationErrorType.NotFound);
            }

            return OperationResult<Order>.Success(order);
        }

        public async Task<OperationResult<Order>> Checkout(int userId)
        {
            var cart = await cartRepository.GetCart(userId);

            if (cart is null || cart.Items.Count == 0)
            {
                return OperationResult<Order>.Failure("Cart is empty.",
                    OperationErrorType.BadRequest);
            }

            var user = await userRepository.GetUserById(userId);
            if (user is null)
            {
                return OperationResult<Order>.Failure("User is not found.",
                   OperationErrorType.NotFound);
            }

            var orderItems = new List<OrderItem>();
            var totalPrice = 0m;
            foreach (var item in cart.Items)
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
                    GameName = game.Name,
                    Game = game
                });

                totalPrice += item.Quantity * game.Price;
            }

            if (user.Balance < totalPrice)
            {
                return OperationResult<Order>.Failure("User doesn't have enough money.",
                   OperationErrorType.BadRequest);
            }

            var order = await orderRepository.Checkout(user, cart, totalPrice, orderItems);
            return OperationResult<Order>.Success(order);
        }

        public async Task<OperationResult<Order>> Refund(int userId, int id)
        {
            var order = await orderRepository.GetOrder(id);
            if (order is null)
            {
                return OperationResult<Order>.Failure("Order is not found.",
                    OperationErrorType.NotFound);
            }

            if (order.UserId != userId)
            {
                return OperationResult<Order>.Failure("No access to that order from this user.",
                    OperationErrorType.NotFound);
            }

            if (order.OrderStatus is OrderStatus.Refunded)
            {
                return OperationResult<Order>.Failure("Order was refunded already.",
                   OperationErrorType.BadRequest);
            }

            var user = await userRepository.GetUserById(userId);
            if (user is null)
            {
                return OperationResult<Order>.Failure("User is not found.",
                    OperationErrorType.NotFound);
            }
            
            var totalPrice = 0m;
            foreach(var item in order.Items)
            {
                totalPrice+= item.UnitPrice*item.Quantity;
            }

            order = await orderRepository.Refund(user,totalPrice,order);

            return OperationResult<Order>.Success(order);
        }
    }
}