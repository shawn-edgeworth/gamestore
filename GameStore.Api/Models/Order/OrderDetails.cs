namespace GameStore.Api.Models
{
    public class OrderDetails
    {
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDetails> Items { get; set; } = [];
    }
}