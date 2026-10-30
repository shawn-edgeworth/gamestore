using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Data;

public class CartRepository
{
    private readonly GameDbContext dbContext;

    private IQueryable<Cart> CartWithItems =>
           dbContext.Carts
           .Include(x => x.Items)
           .ThenInclude(x => x.Game);

    public CartRepository(GameDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<Cart?> GetCart(int userId)
    {
        return await CartWithItems.FirstOrDefaultAsync(cart => cart.UserId == userId);
    }

    public async Task<Cart> UpdateCart(Cart cart)
    {
        if (cart.Id == 0)
        {
            dbContext.Carts.Add(cart);
        }

        await dbContext.SaveChangesAsync();

        return cart;
    }
}