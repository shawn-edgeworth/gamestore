using FluentValidation;
using GameStore.Api.Data;
using GameStore.Api.Models;

namespace GameStore.Api.Services;

public class CartService
{
    private readonly CartRepository cartRepository;
    private readonly IValidator<AddCartItemRequest> addCartItemValidator;
    private readonly IValidator<UpdateCartItemRequest> updateCartItemValidator;
    private readonly GameRepository gameRepository;

    public CartService(CartRepository cartRepository,
    IValidator<AddCartItemRequest> addCartItemValidator,
    IValidator<UpdateCartItemRequest> updateCartItemValidator,
    GameRepository gameRepository)
    {
        this.cartRepository = cartRepository;
        this.addCartItemValidator = addCartItemValidator;
        this.updateCartItemValidator = updateCartItemValidator;
        this.gameRepository = gameRepository;
    }

    public async Task<OperationResult<Cart>> GetCart(int userId)
    {
        var cart = await cartRepository.GetCart(userId) ?? new Cart() { UserId = userId };
        return OperationResult<Cart>.Success(cart);
    }

    public async Task<OperationResult<Cart>> AddItem(int userId,
         AddCartItemRequest request)
    {
        var validation = await addCartItemValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return OperationResult<Cart>.Failure(validation.Errors[0].ErrorMessage,
             OperationErrorType.Validation);
        }

        var game = await gameRepository.GetGameById(request.GameId);
        if (game is null)
        {
            return OperationResult<Cart>.Failure("Game is not found.",
             OperationErrorType.NotFound);
        }

        var cart = await cartRepository.GetCart(userId) ?? new Cart { UserId = userId };
        var cartItem = cart.Items.FirstOrDefault(x => x.GameId == request.GameId);

        if (cartItem is null)
        {
            cartItem = new CartItem
            {
                Game = game,
                GameId = request.GameId,
                Quantity = request.Quantity
            };
            cart.Items.Add(cartItem);
        }
        else
        {
            cartItem.Quantity += request.Quantity;
        }

        cart = await cartRepository.UpdateCart(cart);
        return OperationResult<Cart>.Success(cart);
    }

    public async Task<OperationResult<Cart>> RemoveItem(int userId, int gameId)
    {
        var cart = await cartRepository.GetCart(userId);
        if (cart is null)
        {
            return OperationResult<Cart>.Failure("Cart is empty.",
            OperationErrorType.NotFound);
        }

        var cartItem = cart.Items.FirstOrDefault(x => x.GameId == gameId);

        if (cartItem is null)
        {
            return OperationResult<Cart>.Failure($"Cart doesn't contains game with id={gameId}",
            OperationErrorType.NotFound);
        }

        cart.Items.Remove(cartItem);
        await cartRepository.UpdateCart(cart);
        return OperationResult<Cart>.Success(cart);
    }

    public async Task<OperationResult<Cart>> UpdateItem(int userId, int gameId, UpdateCartItemRequest request)
    {
        var validation = await updateCartItemValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return OperationResult<Cart>.Failure(validation.Errors[0].ErrorMessage,
             OperationErrorType.Validation);
        }

        var cart = await cartRepository.GetCart(userId);
        if (cart is null)
        {
            return OperationResult<Cart>.Failure("Cart is empty.",
            OperationErrorType.NotFound);
        }

        var cartItem = cart.Items.FirstOrDefault(x => x.GameId == gameId);

        if (cartItem is null)
        {
            return OperationResult<Cart>.Failure($"Cart doesn't contains game with id={gameId}",
            OperationErrorType.NotFound);
        }

        cartItem.Quantity = request.Quantity;
        await cartRepository.UpdateCart(cart);
        return OperationResult<Cart>.Success(cart);
    }
}