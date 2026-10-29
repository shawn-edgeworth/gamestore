using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Data;

public class UserRepository
{
    private readonly GameDbContext _dbContext;

    public UserRepository(GameDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsUserExists(string email)
    {
        return await _dbContext.Users.AnyAsync(x => x.Email == email);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<User?> GetUserById(int id)
    {
        return await _dbContext.Users.FindAsync(id);
    }

    public async Task<User> CreateUser(User user)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User?> AddMoney(int userId, decimal amount)
    {
        var user = await GetUserById(userId);
        if (user is null)
        {
            return null;
        }

        user.Balance += amount;
        await _dbContext.SaveChangesAsync();
        return user;
    }
}