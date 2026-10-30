namespace GameStore.Api.Models
{
    public class CreateOrderResponse
    {
        public int Id { get; set; }
        public OrderStatus OrderStatus {get; set;}
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDetails> Items { get; set; } = [];
    }
}