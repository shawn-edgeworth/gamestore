using GameStore.Api.Models;
namespace GameStore.Api.Mappers;


public static class CartMapper
{
    public static CartCollectionResponse ToCollectionResponse(this Cart cart)
    {
        return new CartCollectionResponse
        {
            CartItems = cart.Items.Select(cartItem => cartItem.ToDetails()).ToList()
        };
    }

    public static CartItemDetails ToDetails(this CartItem cartItem)
    {
        return new CartItemDetails
        {
            GameId = cartItem.GameId,
            GameName = cartItem.Game.Name,
            GameImageUrl = cartItem.Game.ImageUrl ?? string.Empty,
            Quantity = cartItem.Quantity,
            UnitPrice = cartItem.Game.Price
        };
    }
}