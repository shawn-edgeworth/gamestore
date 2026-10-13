using GameStore.Api.Models;

namespace GameStore.Api.Mappers;

public static class GameMapper
{
    public static GameDetailsResponse ToDetailsResponse(this Game game)
    {
        return new GameDetailsResponse
        {
            Id = game.Id,
            Name = game.Name,
            Price = game.Price,
            Description = game.Description,
            ReleaseDate = game.ReleaseDate,
            ImageUrl = game.ImageUrl,
            CategoryId = game.CategoryId,
            CategoryName = game.Category?.Name
        };
    }

    public static GameCollectionResponse ToCollectionResponse(this IList<Game> games)
    {
        return new GameCollectionResponse()
        {
            Games = games.Select(x => x.ToDetailsResponse()).ToList()
        };
    }
}