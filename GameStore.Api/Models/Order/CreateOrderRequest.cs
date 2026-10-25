namespace GameStore.Api.Models.Orders
{

    public class CreateOrderRequest
    {
        public List<OrderItemRequest> OrderItems { get; set; } = [];
    }
}