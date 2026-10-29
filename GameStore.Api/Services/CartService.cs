using GameStore.Api.Data;
using GameStore.Api.Models;

namespace GameStore.Api.Services;

public class CartService
{
    private readonly CartRepository cartRepository;

    public CartService(CartRepository cartRepository)
    {
        this.cartRepository = cartRepository;
    }

    public async Task<OperationResult<Cart>> GetCart(int userId)
    {
        var cart = await cartRepository.GetCart(userId)?? new Cart();
        return OperationResult<Cart>.Success(cart);
    }
}