namespace GameStore.Api.Models;

public class OrderItemRequest
{
    public int GameId { get; set; }
    public int Quantity { get; set; }
}