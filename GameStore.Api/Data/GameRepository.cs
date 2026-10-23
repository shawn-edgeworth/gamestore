using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Data;

public class GameRepository
{
    private readonly GameDbContext _dbContext;

    public GameRepository(GameDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Game>> GetAllGames(GameQuery gameQuery)
    {
        IQueryable<Game> query = _dbContext.Games
        .Include(x => x.Category);

        if (gameQuery.CategoryId is not null)
        {
            query = query.Where(x => x.CategoryId == gameQuery.CategoryId);
        }

        if (gameQuery.MinPrice is not null)
        {
            query = query.Where(x => x.Price >= gameQuery.MinPrice);
        }

        if (gameQuery.MaxPrice is not null)
        {
            query = query.Where(x => x.Price <= gameQuery.MaxPrice);
        }

        if (gameQuery.Search is not null)
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{gameQuery.Search}%"));
        }

        if (gameQuery.Page is >= 1 && gameQuery.PageSize >= 1)
        {
            var maxPageSize = Math.Min(gameQuery.PageSize.Value, 50);
            query = query
                    .OrderBy(x => x.Id)
                    .Skip((gameQuery.Page.Value - 1) * maxPageSize)
                    .Take(maxPageSize);
        }

        return await query.ToListAsync();
    }

    public async Task<Game?> GetGameById(int id)
    {
        return await _dbContext.Games
        .Include(x => x.Category)
        .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Game> CreateGame(Game game)
    {
        _dbContext.Games.Add(game);

        await _dbContext.SaveChangesAsync();

        return game;
    }

    public async Task<bool> DeleteGame(int id)
    {
        var game = await _dbContext.Games.FindAsync(id);

        if (game is null)
        {
            return false;
        }

        _dbContext.Games.Remove(game);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<Game?> UpdateGame(int id, UpdateGameRequest request)
    {
        var game = await _dbContext.Games.FindAsync(id);

        if (game is null)
        {
            return null;
        }

        game.Name = request.Name;
        game.Price = request.Price;
        game.Description = request.Description;
        game.ReleaseDate = request.ReleaseDate;
        game.ImageUrl = request.ImageUrl;
        game.CategoryId = request.CategoryId;

        await _dbContext.SaveChangesAsync();

        return game;
    }

    public async Task<Game?> UpdateGamePrice(int id, decimal price)
    {
        var game = await _dbContext.Games.FindAsync(id);

        if (game is null)
        {
            return null;
        }

        game.Price = price;

        await _dbContext.SaveChangesAsync();

        return game;
    }

    public async Task<Game?> UpdateGameCategory(int gameId, int categoryId)
    {
        var game = await _dbContext.Games.FindAsync(gameId);

        if (game is null)
        {
            return null;
        }

        var category = await _dbContext.Categories.FindAsync(categoryId);

        if (category is null)
        {
            return null;
        }

        game.CategoryId = categoryId;
        game.Category = category;

        await _dbContext.SaveChangesAsync();

        return game;
    }
}