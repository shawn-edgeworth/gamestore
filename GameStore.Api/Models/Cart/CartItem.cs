namespace GameStore.Api.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int GameId { get; set; }
        public int Quantity { get; set; }

        public Game Game { get; set; } = null!;
    }
}