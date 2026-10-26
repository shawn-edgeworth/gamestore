using FluentValidation;
using GameStore.Api.Data;
using GameStore.Api.Models;

namespace GameStore.Api.Services;

public class GameService
{
    private readonly GameRepository _gameRepository;
    private readonly CategoryRepository _categoryRepository;
    private readonly IValidator<CreateGameRequest> _createGameValidator;
    private readonly IValidator<UpdateGameRequest> _updateGameValidator;
    private readonly IValidator<UpdateGamePriceRequest> _updateGamePriceValidator;
    private readonly IValidator<UpdateGameCategoryRequest> _updateGameCategoryValidator;

    public GameService(GameRepository gameRepository,
    CategoryRepository categoryRepository,
    IValidator<CreateGameRequest> createGameValidator,
    IValidator<UpdateGameRequest> updateGameValidator,
    IValidator<UpdateGamePriceRequest> updateGamePriceValidator,
    IValidator<UpdateGameCategoryRequest> updateGameCategoryValidator)
    {
        _gameRepository = gameRepository;
        _categoryRepository = categoryRepository;
        _createGameValidator = createGameValidator;
        _updateGameValidator = updateGameValidator;
        _updateGamePriceValidator = updateGamePriceValidator;
        _updateGameCategoryValidator = updateGameCategoryValidator;
    }

    public async Task<List<Game>> GetAllGames(GameQuery gameQuery)
    {
        return await _gameRepository.GetAllGames(gameQuery);
    }

    public async Task<Game?> GetGameById(int id)
    {
        return await _gameRepository.GetGameById(id);
    }

    public async Task<OperationResult<Game>> CreateGame(CreateGameRequest request)
    {
        var validation = await _createGameValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return OperationResult<Game>.Failure(validation.Errors[0].ErrorMessage,
             OperationErrorType.Validation);
        }

        var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);

        if (!categoryExists)
        {
            return OperationResult<Game>.Failure("Category is not found.",
             OperationErrorType.NotFound);
        }

        var game = new Game
        {
            Name = request.Name,
            Price = request.Price,
            Description = request.Description,
            ReleaseDate = request.ReleaseDate,
            ImageUrl = request.ImageUrl,
            CategoryId = request.CategoryId
        };

        var createdGame = await _gameRepository.CreateGame(game);

        if (createdGame is null)
        {
            return OperationResult<Game>.Failure("Game is not found", OperationErrorType.NotFound);
        }

        var selectedGame = await _gameRepository.GetGameById(createdGame.Id);

        if (selectedGame is null)
        {
            return OperationResult<Game>.Failure("Game is not found.", OperationErrorType.NotFound);
        }

        return OperationResult<Game>.Success(selectedGame);
    }

    public async Task<OperationResult<bool>> DeleteGame(int id)
    {
        var isDeleted = await _gameRepository.DeleteGame(id);
        return isDeleted ?
        OperationResult<bool>.Success(isDeleted) :
        OperationResult<bool>.Failure("Game is not found.", OperationErrorType.NotFound);
    }

    public async Task<OperationResult<Game>> UpdateGame(int id, UpdateGameRequest request)
    {
        var validation = await _updateGameValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return OperationResult<Game>.Failure(validation.Errors[0].ErrorMessage,
             OperationErrorType.Validation);
        }

        var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);

        if (!categoryExists)
        {
            return OperationResult<Game>.Failure("Category is not found.",
             OperationErrorType.NotFound);
        }

        var updatedGame = await _gameRepository.UpdateGame(id, request);

        if (updatedGame is null)
        {
            return OperationResult<Game>.Failure("Game is not found", OperationErrorType.NotFound);
        }

        var selectedGame = await _gameRepository.GetGameById(updatedGame.Id);

        if (selectedGame is null)
        {
            return OperationResult<Game>.Failure("Game is not found.", OperationErrorType.NotFound);
        }

        return OperationResult<Game>.Success(selectedGame);
    }

    public async Task<OperationResult<Game>> UpdateGamePrice(int id, UpdateGamePriceRequest request)
    {
        var validation = await _updateGamePriceValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return OperationResult<Game>.Failure(validation.Errors[0].ErrorMessage,
             OperationErrorType.Validation);
        }
        

        var game = await _gameRepository.UpdateGamePrice(id, request.Price);

        if (game is null)
        {
            return OperationResult<Game>.Failure("Game is not found", OperationErrorType.NotFound);
        }
        return OperationResult<Game>.Success(game);
    }

    public async Task<OperationResult<Game>> UpdateGameCategory(int id, UpdateGameCategoryRequest request)
    {
        var validation = await _updateGameCategoryValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return OperationResult<Game>.Failure(validation.Errors[0].ErrorMessage,
             OperationErrorType.Validation);
        }

        var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);

        if (!categoryExists)
        {
            return OperationResult<Game>.Failure("Category is not found.",
             OperationErrorType.NotFound);
        }

        var game = await _gameRepository.UpdateGameCategory(id, request.CategoryId);

        if (game is null)
        {
            return OperationResult<Game>.Failure("Game is not found", OperationErrorType.NotFound);
        }

        return OperationResult<Game>.Success(game);
    }
}