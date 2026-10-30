namespace GameStore.Api.Models;

public class AddCartItemRequest
{
    public int GameId {get; set;}
    public int Quantity {get; set;}
}