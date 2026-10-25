namespace GameStore.Api.Models
{
    public class OrderItemDetails
    {
        public int GameId { get; set; }
        public string GameName {get; set;} = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}