namespace GameStore.Api.Models.Orders
{
    public class OrderItemRequest
    {
        public int GameId { get; set; }
        public int Quantity { get; set; }
    }
}