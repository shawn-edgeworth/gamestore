using FluentValidation;
using GameStore.Api.Data;
using GameStore.Api.Models;

namespace GameStore.Api.Services;

public class CategoryService
{
    private readonly CategoryRepository _categoryRepository;
    private readonly IValidator<CreateCategoryRequest> _createCategoryValidator;
    private readonly IValidator<UpdateCategoryRequest> _updateCategoryValidator;

    public CategoryService(CategoryRepository categoryRepository,
    IValidator<CreateCategoryRequest> createCategoryValidator,
    IValidator<UpdateCategoryRequest> updateCategoryValidator)
    {
        _categoryRepository = categoryRepository;
        _createCategoryValidator = createCategoryValidator;
        _updateCategoryValidator = updateCategoryValidator;
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
        var validation = await _createCategoryValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return OperationResult<Category>.Failure(validation.Errors[0].ErrorMessage, OperationErrorType.Validation);
        }

        var category = new Category()
        {
            Name = request.Name.Trim()
        };

        await _categoryRepository.CreateCategory(category);

        return OperationResult<Category>.Success(category);
    }

    public async Task<OperationResult<bool>> DeleteCategory(int id)
    {
        var isDeleted = await _categoryRepository.DeleteCategory(id);

        return isDeleted ?
        OperationResult<bool>.Success(isDeleted) :
        OperationResult<bool>.Failure("Category is not found.", OperationErrorType.NotFound);
    }

    public async Task<OperationResult<Category>> UpdateCategory(int id, UpdateCategoryRequest request)
    {
        var validation = await _updateCategoryValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return OperationResult<Category>.Failure(validation.Errors[0].ErrorMessage, OperationErrorType.Validation);
        }

        var updatedCategory = await _categoryRepository.UpdateCategory(id, request);

        if (updatedCategory is null)
        {
            return OperationResult<Category>.Failure("Category is not found.", OperationErrorType.NotFound);
        }

        var selectedCategory = await _categoryRepository.GetCategoryById(updatedCategory.Id);

        if (selectedCategory is null)
        {
            return OperationResult<Category>.Failure("Category is not found", OperationErrorType.NotFound);
        }

        return OperationResult<Category>.Success(selectedCategory);
    }
}