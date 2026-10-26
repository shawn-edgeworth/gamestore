using System.Net;
using System.Net.Http.Json;
using GameStore.Api.Models;

namespace GameStore.Api.Tests;

public class CategoryTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;

    public CategoryTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
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
            PostCategory("Action"),
            PostCategory("Adventure"),
            PostCategory("RPG"),
        };

        await Task.WhenAll(gameGenres);
    }

    private Task<HttpResponseMessage> PostCategory(string name)
    {
        var createRequest = new CreateCategoryRequest
        {
            Name = name
        };

        return _client.PostAsJsonAsync("/categories", createRequest);
    }

    [Fact]
    public async Task GetCategory_ValidId_ReturnOK()
    {
        var getResponse = await _client.GetAsync(
            $"/categories/{1}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);
    }

    [Fact]
    public async Task GetCategory_InvalidId_ReturnNotFound()
    {
        var getResponse = await _client.GetAsync(
           $"/categories/{int.MinValue}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_ValidName_ReturnCreated()
    {
        var response = await PostCategory("Racing");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PutCategory_ValidName_ReturnNoContent()
    {
        var putRequest = new UpdateCategoryRequest()
        {
            Name = "Role-Playing Game (RPG)"
        };

        var putResponse = await _client.PutAsJsonAsync($"/categories/{3}", putRequest);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        var updatedCategory = await putResponse.Content
           .ReadFromJsonAsync<Category>();

        Assert.NotNull(updatedCategory);
        Assert.Equal("Role-Playing Game (RPG)", updatedCategory.Name);
    }

    
}