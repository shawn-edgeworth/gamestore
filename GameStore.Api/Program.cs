using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using GameStore.Api.Data;
using GameStore.Api.Mappers;
using GameStore.Api.Models;
using GameStore.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;



var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
                   Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = "GameStore",
        ValidateAudience = true,
        ValidAudience = "GameStore.Api",
        ValidateLifetime = true
    };
});

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

builder.Services.AddAuthorization();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddScoped<GameRepository>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<CartRepository>();
builder.Services.AddScoped<CartService>();

builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateGameRequest>();

builder.Services.AddDbContext<GameDbContext>(options =>
      options.UseNpgsql(builder.Configuration.GetConnectionString("GameStore")));
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "GameStore API");
    });
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

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

    var game = result.Value;

    return Results.Created($"/games/{game.Id}", game.ToDetailsResponse());
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapDelete("/games/{id}", async (GameService gameService, int id) =>
{
    var result = await gameService.DeleteGame(id);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.NoContent();
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPut("/games/{id}", async (GameService gameService, int id, UpdateGameRequest request) =>
{
    var result = await gameService.UpdateGame(id, request);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.NoContent();
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPatch("/games/{id}/price", async (GameService gameService, int id, UpdateGamePriceRequest request) =>
{
    var result = await gameService.UpdateGamePrice(id, request);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.Ok(result.Value.ToDetailsResponse());
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPatch("/games/{id}/category", async (GameService gameService, int id, UpdateGameCategoryRequest request) =>
{
    var result = await gameService.UpdateGameCategory(id, request);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.Ok(result.Value.ToDetailsResponse());
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

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


    var category = result.Value;

    return Results.Created($"/categories/{category.Id}", category.ToDetailsResponse());
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapDelete("/categories/{id}", async (CategoryService categoryService, int id) =>
{
    var result = await categoryService.DeleteCategory(id);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.NoContent();
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPut("/categories/{id}", async (CategoryService categoryService, int id, UpdateCategoryRequest request) =>
{
    var result = await categoryService.UpdateCategory(id, request);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.NoContent();
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPost("/register", async (UserService userService, UserRegisterRequest request) =>
{
    var result = await userService.Register(request);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    var user = result.Value;

    return Results.Created($"/users/{user.Id}", user.ToDetailsResponse());
});

app.MapPost("/login", async (UserService userService, UserLoginRequest request) =>
{
    var result = await userService.Login(request);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    var loginResponse = result.Value;

    return Results.Ok(loginResponse);
});

app.MapPost("/orders", async (OrderService orderService, ClaimsPrincipal user, CreateOrderRequest request) =>
{
    if (!TryGetUserId(user, out var userId))
    {
        return ToHttpError(OperationErrorType.Authorization, "User is not found.");
    }

    var result = await orderService.CreateOrder(userId, request);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.Created($"/orders/{result.Value.Id}", result.Value.ToResponse());
}).RequireAuthorization();

app.MapGet("/orders", async (OrderService orderService, ClaimsPrincipal user) =>
{
    if (!TryGetUserId(user, out var userId))
    {
        return ToHttpError(OperationErrorType.Authorization, "User is not found.");
    }

    var result = await orderService.GetOrders(userId);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.Ok(result.Value.ToCollectionResponse());
}).RequireAuthorization();

app.MapGet("/orders/{id}", async (int id, OrderService orderService, ClaimsPrincipal user) =>
{
   if (!TryGetUserId(user, out var userId))
    {
        return ToHttpError(OperationErrorType.Authorization, "User is not found.");
    }

    var result = await orderService.GetOrder(id, userId);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.Ok(result.Value.ToResponse());
}).RequireAuthorization();

app.MapPost("/orders/{id}/refund", async (int id, OrderService orderService, ClaimsPrincipal user) =>
{
    if (!TryGetUserId(user, out var userId))
    {
        return ToHttpError(OperationErrorType.Authorization, "User is not found.");
    }

    var result = await orderService.Refund(userId,id);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.Ok(result.Value.ToResponse());
}).RequireAuthorization();


app.MapGet("/users/me", async (ClaimsPrincipal user, UserService userService) =>
{
    if (!TryGetUserId(user, out var userId))
    {
        return ToHttpError(OperationErrorType.Authorization, "User is not found.");
    }

    var result = await userService.GetCurrentUser(userId);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.Ok(result.Value.ToDetailsResponse());
}).RequireAuthorization();

app.MapPost("/users/me/balance", async (ClaimsPrincipal user, UserService userService, UserAddMoneyRequest request) =>
{
    if (!TryGetUserId(user, out var userId))
    {
        return ToHttpError(OperationErrorType.Authorization, "User is not found.");
    }

    var result = await userService.AddMoney(userId, request);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.Ok(result.Value.ToDetailsResponse());
}).RequireAuthorization();

app.MapGet("/cart", async (ClaimsPrincipal user, CartService cartService) =>
{
    if (!TryGetUserId(user, out var userId))
    {
        return ToHttpError(OperationErrorType.Authorization, "User is not found.");
    }

    var result = await cartService.GetCart(userId);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.Ok(result.Value.ToCollectionResponse());
}).RequireAuthorization();

app.MapPost("/cart/items", async (ClaimsPrincipal user, CartService cartService, AddCartItemRequest request) =>
{
    if (!TryGetUserId(user, out var userId))
    {
        return ToHttpError(OperationErrorType.Authorization, "User is not found.");
    }

    var result = await cartService.AddItem(userId, request);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.Ok(result.Value.ToCollectionResponse());
}).RequireAuthorization();

app.MapDelete("/cart/items/{gameId}", async (ClaimsPrincipal user, CartService cartService, int gameId) =>
{
    if (!TryGetUserId(user, out var userId))
    {
        return ToHttpError(OperationErrorType.Authorization, "User is not found.");
    }

    var result = await cartService.RemoveItem(userId, gameId);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.Ok(result.Value.ToCollectionResponse());
}).RequireAuthorization();

app.MapPatch("/cart/items/{gameId}", async (ClaimsPrincipal user,
 CartService cartService, int gameId, UpdateCartItemRequest request) =>
{
    if (!TryGetUserId(user, out var userId))
    {
        return ToHttpError(OperationErrorType.Authorization, "User is not found.");
    }

    var result = await cartService.UpdateItem(userId, gameId, request);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.Ok(result.Value.ToCollectionResponse());
}).RequireAuthorization();

app.MapPost("/cart/checkout", async (ClaimsPrincipal user, OrderService orderService) =>
{
    if (!TryGetUserId(user, out var userId))
    {
        return ToHttpError(OperationErrorType.Authorization, "User is not found.");
    }

    var result = await orderService.Checkout(userId);

    if (!result.IsSuccess)
    {
        return ToHttpError(result.ErrorType, result.Error);
    }

    return Results.Created($"/orders/{result.Value.Id}", result.Value.ToResponse());
}).RequireAuthorization();

app.Run();

static IResult ToHttpError(OperationErrorType errorType, string? error)
{
    return errorType switch
    {
        OperationErrorType.Validation => Results.BadRequest(error),
        OperationErrorType.NotFound => Results.NotFound(error),
        OperationErrorType.Authorization => Results.Unauthorized(),
        OperationErrorType.BadRequest => Results.BadRequest(error),
        _ => Results.Problem(statusCode: StatusCodes.Status500InternalServerError)
    };
}

static bool TryGetUserId(ClaimsPrincipal user, out int userId)
{
    return Int32.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}

public partial class Program { }

