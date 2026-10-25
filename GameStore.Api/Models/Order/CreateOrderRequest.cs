namespace GameStore.Api.Models;

public class CreateOrderRequest
{
    public List<OrderItemRequest> OrderItems { get; set; } = [];
}