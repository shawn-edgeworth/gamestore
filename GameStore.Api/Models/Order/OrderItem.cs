namespace GameStore.Api.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int GameId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string GameName { get; set; } = string.Empty;

        public Game Game { get; set; } = null!;
    }
}