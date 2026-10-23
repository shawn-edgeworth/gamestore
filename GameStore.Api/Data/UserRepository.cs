using GameStore.Api.Data;
using Microsoft.EntityFrameworkCore;

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

    public async Task<User> CreateUser(User user)
    {
        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync();

        return user;
    }
}