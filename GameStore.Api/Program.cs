using System.Text.Json.Serialization;
using FluentValidation;
using GameStore.Api.Data;
using GameStore.Api.Mappers;
using GameStore.Api.Models;
using GameStore.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

builder.Services.AddScoped<GameRepository>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<CategoryService>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateGameRequest>();

builder.Services.AddDbContext<GameDbContext>(options =>
      options.UseNpgsql(builder.Configuration.GetConnectionString("GameStore")));
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "GameStore API");
    });
}

app.UseHttpsRedirection();

app.MapGet("/games", async (GameService gameService, [AsParameters] GameQuery gameQuery) =>
{
    var games = await gameService.GetAllGames(gameQuery);
    return Results.Ok(games.ToCollectionResponse());
});

app.MapGet("/games/{id}", async (GameService gameService, int id) =>
{
    var game = await gameService.GetGameById(id);

    if (game is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(game.ToDetailsResponse());
});

app.MapPost("/games", async (GameService gameService, CreateGameRequest request) =>
{
    var result = await gameService.CreateGame(request);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    if (result.Value is null)
    {
        return ToHttpError(OperationErrorType.NotFound, "Game is not found.");
    }

    var game = result.Value;

    return Results.Created($"/games/{game.Id}", game.ToDetailsResponse());
});

app.MapDelete("/games/{id}", async (GameService gameService, int id) =>
{
    var result = await gameService.DeleteGame(id);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.NoContent();
});

app.MapPut("/games/{id}", async (GameService gameService, int id, UpdateGameRequest request) =>
{
    var result = await gameService.UpdateGame(id, request);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.NoContent();
});

app.MapPatch("/games/{id}/price", async (GameService gameService, int id, UpdateGamePriceRequest request) =>
{
    var result = await gameService.UpdateGamePrice(id, request);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    if (result.Value is null)
    {
        return ToHttpError(OperationErrorType.NotFound, "Game is not found.");
    }

    return Results.Ok(result.Value.ToDetailsResponse());
});

app.MapPatch("/games/{id}/category", async (GameService gameService, int id, UpdateGameCategoryRequest request) =>
{
    var result = await gameService.UpdateGameCategory(id, request);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    if (result.Value is null)
    {
        return ToHttpError(
            OperationErrorType.NotFound,
            "Game is not found.");
    }

    return Results.Ok(result.Value.ToDetailsResponse());
});

app.MapGet("/categories", async (CategoryService categoryService) =>
{
    var categories = await categoryService.GetAllCategories();
    return Results.Ok(categories.ToCollectionResponse());
});

app.MapGet("/categories/{id}", async (CategoryService categoryService, int id) =>
{
    var category = await categoryService.GetCategoryById(id);

    if (category is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(category.ToDetailsResponse());
});

app.MapPost("/categories", async (CategoryService categoryService, CreateCategoryRequest request) =>
{
    var result = await categoryService.CreateCategory(request);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    if (result.Value is null)
    {
        return ToHttpError(OperationErrorType.NotFound, "Category is not found.");
    }

    var category = result.Value;

    return Results.Created($"/categories/{category.Id}", category.ToDetailsResponse());
});

app.MapDelete("/categories/{id}", async (CategoryService categoryService, int id) =>
{
    var result = await categoryService.DeleteCategory(id);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.NoContent();
});

app.MapPut("/categories/{id}", async (CategoryService categoryService, int id, UpdateCategoryRequest request) =>
{
    var result = await categoryService.UpdateCategory(id, request);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.NoContent();
});

app.Run();

static IResult ToHttpError(OperationErrorType errorType, string? error)
{
    return errorType switch
    {
        OperationErrorType.Validation => Results.BadRequest(error),
        OperationErrorType.NotFound => Results.NotFound(error),
        _ => Results.Problem(statusCode: StatusCodes.Status500InternalServerError)
    };
}

public partial class Program { }

