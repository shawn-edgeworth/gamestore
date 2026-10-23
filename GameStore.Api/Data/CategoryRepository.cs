using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace GameStore.Api.Data;

public class CategoryRepository
{
    private readonly GameDbContext _dbContext;

    public CategoryRepository(GameDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Category>> GetAllCategories()
    {
        return await _dbContext.Categories
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryById(int id)
    {
        return await _dbContext.Categories.FindAsync(id);
    }

    public async Task<Category> CreateCategory(Category category)
    {
        _dbContext.Categories.Add(category);

        await _dbContext.SaveChangesAsync();

        return category;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        var category = await _dbContext.Categories.FindAsync(id);
        return category != null;
    }

    public async Task<bool> DeleteCategory(int id)
    {
        var category = await _dbContext.Categories.FindAsync(id);

        if (category is null)
        {
            return false;
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<Category?> UpdateCategory(int id, UpdateCategoryRequest request)
    {
        var category = await _dbContext.Categories.FindAsync(id);

        if (category is null)
        {
            return null;
        }

        category.Name = request.Name.Trim();
        await _dbContext.SaveChangesAsync();

        return category;
    }
}