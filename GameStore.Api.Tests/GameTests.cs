using System.Net;
using System.Net.Http.Json;
using GameStore.Api.Models;

namespace GameStore.Api.Tests;

public class GameTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient client;
    private readonly Random random = new Random((int)DateTime.UtcNow.Ticks);

    public GameTests(CustomWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await Seed();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task Seed()
    {
        var gameGenres = new Task[]
        {
            PostCategory("Platformer"),
            PostCategory("Adventure"),
            PostCategory("RPG"),
        };

        await Task.WhenAll(gameGenres);

        var games = new Task[]
        {
            PostGame("Crash Bandicoot",9.99m,categoryId:1),
            PostGame("Sonic The Hedgehog",4.99m,categoryId:2),
            PostGame("Arknights Endfield",15.99m,categoryId:3),
        };

        await Task.WhenAll(games);
    }

    private Task<HttpResponseMessage> PostCategory(string name)
    {
        var createRequest = new CreateCategoryRequest
        {
            Name = name
        };

        return client.PostAsJsonAsync("/categories", createRequest);
    }

    private Task<HttpResponseMessage> PostGame(string name, decimal price,
        string? description = null,
        string? imageUrl = null,
        DateOnly? date = null,
        int? categoryId = null)
    {
        var request = new CreateGameRequest
        {
            Name = name,
            Price = price,
            Description = description is null ? $"{name}'s description" : description,
            ImageUrl = imageUrl is null ? $@"\images\{name}.png" : imageUrl,
            ReleaseDate = date is null ? new DateOnly(random.Next(2000, 2022), random.Next(1, 12), random.Next(1, 31)) : date,
            CategoryId = categoryId is null ? random.Next(1, 3) : categoryId.Value
        };

        return client.PostAsJsonAsync("/games", request);
    }

    [Fact]
    public async Task PostGame_ValidRequest_ReturnsCreated()
    {
        var response = await PostGame("Mario Party", 25.99m, "New Game About Mario Adventures from Nintendo!",
        @"\images\marioParty.png", new DateOnly(2026, 5, 15), 1);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var game = await response.Content
            .ReadFromJsonAsync<Game>();

        Assert.NotNull(game);
        Assert.Equal("Mario Party", game.Name);
        Assert.Equal(25.99m, game.Price);
        Assert.Equal("New Game About Mario Adventures from Nintendo!", game.Description);
        Assert.Equal(@"\images\marioParty.png", game.ImageUrl);
        Assert.Equal(new DateOnly(2026, 5, 15), game.ReleaseDate);
        Assert.Equal(1, game.CategoryId);

        var getResponse = await client.GetAsync(
            $"/games/{game.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);
    }

    [Fact]
    public async Task GetGame_NonExistingId_ReturnsNotFound()
    {
        var response = await client.GetAsync(
            $"/games/{int.MinValue}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task PostGame_NegativePrice_ReturnsBadRequest()
    {
        var response = await PostGame("Mario Party", -100m, "New Game About Mario Adventures from Nintendo!",
        @"\images\marioParty.png", new DateOnly(2026, 5, 15), 1);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task DeleteGame_ExistingId_ReturnNoContent()
    {
        var deleteResponse = await client.DeleteAsync(
            $"/games/{1}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        var getResponse = await client.GetAsync(
            $"/games/{1}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode
        );
    }

    [Fact]
    public async Task PutGame_ReturnNoContent_ActualyUpdateGame()
    {
        var updateGameRequest = new UpdateGameRequest()
        {
            Name = "Crash Bandicoot Bundle",
            Price = 29.99m,
            Description = "Crash Bandicoot contains now two games in one!",
            ImageUrl = @"\images\crashBandicootBundle.png",
            ReleaseDate = new DateOnly(2026, 1, 28),
            CategoryId = 2
        };

        var putResponse = await client.PutAsJsonAsync(
            $"/games/{1}", updateGameRequest);

        Assert.Equal(
            HttpStatusCode.NoContent,
            putResponse.StatusCode
        );

        var getResponse = await client.GetAsync(
            $"/games/{1}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode
        );

        var updatedGame = await getResponse.Content.ReadFromJsonAsync<Game>();

        Assert.NotNull(updatedGame);
        Assert.Equal("Crash Bandicoot Bundle", updatedGame.Name);
        Assert.Equal(29.99m, updatedGame.Price);
        Assert.Equal("Crash Bandicoot contains now two games in one!", updatedGame.Description);
        Assert.Equal(@"\images\crashBandicootBundle.png", updatedGame.ImageUrl);
        Assert.Equal(new DateOnly(2026, 1, 28), updatedGame.ReleaseDate);
        Assert.Equal(2, updatedGame.CategoryId);
    }

    [Fact]
    public async Task PutGame_NotExistingId_ReturnNotFound()
    {
        var updateGameRequest = new UpdateGameRequest()
        {
            Name = "Crash Bandicoot Bundle",
            Price = 29.99m,
            Description = "Crash Bandicoot contains now two games in one!",
            ImageUrl = @"\images\crashBandicootBundle.png",
            ReleaseDate = new DateOnly(2026, 1, 28),
            CategoryId = 2
        };

        var putResponse = await client.PutAsJsonAsync(
            $"/games/{int.MinValue}", updateGameRequest);

        Assert.Equal(
            HttpStatusCode.NotFound,
            putResponse.StatusCode
        );
    }

    [Fact]
    public async Task PatchGame_UpdatePrice_KeepName()
    {
        var updateGamePriceRequest = new UpdateGamePriceRequest()
        {
            Price = 99.99m
        };

        var patchResponse = await client.PatchAsJsonAsync(
            $"/games/{1}/price", updateGamePriceRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            patchResponse.StatusCode
            );

        var getResponse = await client.GetAsync(
        $"/games/{1}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode
            );

        var updatedGame = await getResponse.Content.ReadFromJsonAsync<Game>();

        Assert.NotNull(updatedGame);

        Assert.Equal(99.99m, updatedGame.Price);
        Assert.Equal("Crash Bandicoot", updatedGame.Name);
    }

    [Fact]
    public async Task PatchGame_NegativePrice_ReturnBadRequest()
    {
        var updateGamePriceRequest = new UpdateGamePriceRequest()
        {
            Price = -100m
        };

        var patchResponse = await client.PatchAsJsonAsync(
            $"/games/{1}/price", updateGamePriceRequest);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            patchResponse.StatusCode
            );

        var getResponse = await client.GetAsync(
        $"/games/{1}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode
            );

        var updatedGame = await getResponse.Content.ReadFromJsonAsync<Game>();

        Assert.NotNull(updatedGame);
        Assert.Equal(19.99m, updatedGame.Price);
    }

    [Fact]
    public async Task PutGame_NegativePrice_ReturnBadRequest()
    {
        var updateGameRequest = new UpdateGameRequest()
        {
            Name = "Crash Bandicoot Bundle",
            Price = -100m,
            Description = "Crash Bandicoot contains now two games in one!",
            ImageUrl = @"\images\crashBandicootBundle.png",
            ReleaseDate = new DateOnly(2026, 1, 28),
            CategoryId = 2
        };

        var putResponse = await client.PutAsJsonAsync(
            $"/games/{1}", updateGameRequest);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            putResponse.StatusCode
            );

        var getResponse = await client.GetAsync(
        $"/games/{1}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode
            );

        var updatedGame = await getResponse.Content.ReadFromJsonAsync<Game>();

        Assert.NotNull(updatedGame);
        Assert.Equal("Crash Bandicoot", updatedGame.Name);
        Assert.Equal(9.99m, updatedGame.Price);
        Assert.Equal(1, updatedGame.CategoryId);
    }

    [Fact]
    public async Task PutGame_EmptyName_ReturnBadRequest()
    {
        var updateGameRequest = new UpdateGameRequest()
        {
            Name = string.Empty,
            Price = -100m,
            Description = "Crash Bandicoot contains now two games in one!",
            ImageUrl = @"\images\crashBandicootBundle.png",
            ReleaseDate = new DateOnly(2026, 1, 28),
            CategoryId = 2
        };


        var putResponse = await client.PutAsJsonAsync(
            $"/games/{1}", updateGameRequest);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            putResponse.StatusCode
            );

        var getResponse = await client.GetAsync(
        $"/games/{1}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode
            );

        var updatedGame = await getResponse.Content.ReadFromJsonAsync<Game>();

        Assert.NotNull(updatedGame);
        Assert.Equal("Crash Bandicoot", updatedGame.Name);
        Assert.Equal(9.99m, updatedGame.Price);
        Assert.Equal(1, updatedGame.CategoryId);
    }

    [Fact]
    public async Task DeleteGame_NotExistingId_ReturnNotFound()
    {
        var deleteResponse = await client.DeleteAsync(
            $"/games/{int.MinValue}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            deleteResponse.StatusCode
            );
    }
}
