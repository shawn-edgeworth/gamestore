using GameStore.Api.Data;
using GameStore.Api.Models;
using GameStore.Api.Validation;

namespace GameStore.Api.Services;

public class CategoryService
{
    private readonly CategoryRepository _categoryRepository;

    public CategoryService(CategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<Category>> GetAllCategories()
    {
        return await _categoryRepository.GetAllCategories();
    }

    public async Task<Category?> GetCategoryById(int id)
    {
        return await _categoryRepository.GetCategoryById(id);
    }

    public async Task<OperationResult<Category>> CreateCategory(CreateCategoryRequest request)
    {
        var result = CategoryValidator.ValidateCategory(request.Name);

        if (result is not CategoryValidationError.None)
        {
            return OperationResult<Category>.Failure(result.ToMessage(), OperationErrorType.Validation);
        }

        var category = new Category()
        {
            Name = request.Name.Trim()
        };

        await _categoryRepository.CreateCategory(category);

        return OperationResult<Category>.Success(category);
    }
}