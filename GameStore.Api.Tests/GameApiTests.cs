using System.Net;
using System.Net.Http.Json;
using GameStore.Api.Data;
using GameStore.Api.Models;

namespace GameStore.Api.Tests;

public class GameApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GameApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostGame_ValidRequest_ReturnsCreated()
    {
        var request = new CreateGameRequest
        {
            Name = "Test Game",
            Price = 19.99m
        };

        var response = await _client.PostAsJsonAsync(
            "/games", request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var game = await response.Content
            .ReadFromJsonAsync<Game>();

        Assert.NotNull(game);
        Assert.Equal("Test Game", game.Name);
        Assert.Equal(19.99m, game.Price);

        var getResponse = await _client.GetAsync(
            $"/games/{game.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);
    }

    [Fact]
    public async Task GetGame_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.GetAsync(
            "/games/999999");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task PostGame_NegativePrice_ReturnsBadRequest()
    {
        var request = new CreateGameRequest
        {
            Name = "Test Game",
            Price = -10m
        };

        var response = await _client.PostAsJsonAsync(
            "/games", request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task DeleteGame_ExistingId_ReturnNoContent()
    {
        var request = new CreateGameRequest
        {
            Name = "Test Game",
            Price = 19.99m
        };

        var postResponse = await _client.PostAsJsonAsync(
            "/games", request);

        var game = await postResponse.Content
            .ReadFromJsonAsync<Game>();

        Assert.NotNull(game);

        var deleteResponse = await _client.DeleteAsync(
            $"/games/{game.Id}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync(
            $"/games/{game.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode
        );
    }

    [Fact]
    public async Task PutGame_ReturnNoContent_ActualyUpdateGame()
    {
        var createRequest = new CreateGameRequest()
        {
            Name = "Test Game",
            Price = 19.99m
        };

        var postResponse = await _client.PostAsJsonAsync(
            "/games", createRequest
        );

        Assert.Equal(
           HttpStatusCode.Created,
           postResponse.StatusCode
       );

        var newGame = await postResponse.Content.ReadFromJsonAsync<Game>();

        Assert.NotNull(newGame);

        var updateGameRequest = new UpdateGameRequest()
        {
            Name = "New Test Game",
            Price = 29.99m
        };

        var putResponse = await _client.PutAsJsonAsync(
            $"/games/{newGame.Id}", updateGameRequest);

        Assert.Equal(
            HttpStatusCode.NoContent,
            putResponse.StatusCode
        );

        var getResponse = await _client.GetAsync(
            $"/games/{newGame.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode
        );

        var updatedGame = await getResponse.Content.ReadFromJsonAsync<Game>();

        Assert.NotNull(updatedGame);
        Assert.Equal("New Test Game", updatedGame.Name);
        Assert.Equal(29.99m, updatedGame.Price);
    }

    [Fact]
    public async Task PutGame_NotExistingId_ReturnNotFound()
    {
        var updateGameRequest = new UpdateGameRequest()
        {
            Name = "New Test Game",
            Price = 29.99m
        };

        var putResponse = await _client.PutAsJsonAsync(
            $"/games/99999999", updateGameRequest);

        Assert.Equal(
            HttpStatusCode.NotFound,
            putResponse.StatusCode
        );
    }

    [Fact]
    public async Task PatchGame_UpdatePrice_KeepName()
    {
        var createRequest = new CreateGameRequest()
        {
            Name = "Test Game",
            Price = 19.99m
        };

        var postResponse = await _client.PostAsJsonAsync(
            "/games", createRequest
        );

        Assert.Equal(
           HttpStatusCode.Created,
           postResponse.StatusCode
       );

        var newGame = await postResponse.Content.ReadFromJsonAsync<Game>();

        Assert.NotNull(newGame);

        var updateGamePriceRequest = new UpdateGamePriceRequest()
        {
            Price = 99.99m
        };

        var patchResponse = await _client.PatchAsJsonAsync(
            $"/games/{newGame.Id}/price", updateGamePriceRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            patchResponse.StatusCode
            );

        var getResponse = await _client.GetAsync(
        $"/games/{newGame.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode
            );

        var updatedGame = await getResponse.Content.ReadFromJsonAsync<Game>();

        Assert.NotNull(updatedGame);

        Assert.Equal(99.99m, updatedGame.Price);
        Assert.Equal("Test Game", updatedGame.Name);
    }

    [Fact]
    public async Task PatchGame_NegativePrice_ReturnBadRequest()
    {
        var createRequest = new CreateGameRequest()
        {
            Name = "Test Game",
            Price = 19.99m
        };

        var postResponse = await _client.PostAsJsonAsync(
            "/games", createRequest
        );

        Assert.Equal(
           HttpStatusCode.Created,
           postResponse.StatusCode
       );

        var newGame = await postResponse.Content.ReadFromJsonAsync<Game>();

        Assert.NotNull(newGame);

        var updateGamePriceRequest = new UpdateGamePriceRequest()
        {
            Price = -1m
        };

        var patchResponse = await _client.PatchAsJsonAsync(
            $"/games/{newGame.Id}/price", updateGamePriceRequest);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            patchResponse.StatusCode
            );

        var getResponse = await _client.GetAsync(
        $"/games/{newGame.Id}");

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
        var createRequest = new CreateGameRequest()
        {
            Name = "Test Game",
            Price = 19.99m
        };

        var postResponse = await _client.PostAsJsonAsync(
            "/games", createRequest
        );

        Assert.Equal(
           HttpStatusCode.Created,
           postResponse.StatusCode
       );

        var newGame = await postResponse.Content.ReadFromJsonAsync<Game>();

        Assert.NotNull(newGame);

        var updateGameRequest = new UpdateGameRequest()
        {
            Name = "New Test Game",
            Price = -1m
        };

        var putResponse = await _client.PutAsJsonAsync(
            $"/games/{newGame.Id}", updateGameRequest);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            putResponse.StatusCode
            );

        var getResponse = await _client.GetAsync(
        $"/games/{newGame.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode
            );

        var updatedGame = await getResponse.Content.ReadFromJsonAsync<Game>();

        Assert.NotNull(updatedGame);
        Assert.Equal("Test Game", updatedGame.Name);
        Assert.Equal(19.99m, updatedGame.Price);
    }

    [Fact]
    public async Task PutGame_EmptyName_ReturnBadRequest()
    {
        var createRequest = new CreateGameRequest()
        {
            Name = "Test Game",
            Price = 19.99m
        };

        var postResponse = await _client.PostAsJsonAsync(
            "/games", createRequest
        );

        Assert.Equal(
           HttpStatusCode.Created,
           postResponse.StatusCode
       );

        var newGame = await postResponse.Content.ReadFromJsonAsync<Game>();

        Assert.NotNull(newGame);

        var updateGameRequest = new UpdateGameRequest()
        {
            Name = string.Empty,
            Price = 49.99m
        };

        var putResponse = await _client.PutAsJsonAsync(
            $"/games/{newGame.Id}", updateGameRequest);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            putResponse.StatusCode
            );

        var getResponse = await _client.GetAsync(
        $"/games/{newGame.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode
            );

        var updatedGame = await getResponse.Content.ReadFromJsonAsync<Game>();

        Assert.NotNull(updatedGame);
        Assert.Equal("Test Game", updatedGame.Name);
        Assert.Equal(19.99m, updatedGame.Price);
    }

    [Fact]
    public async Task DeleteGame_NotExistingId_ReturnNotFound()
    {
        var deleteResponse = await _client.DeleteAsync(
            $"/games/{int.MinValue}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            deleteResponse.StatusCode
            );
    }
}
