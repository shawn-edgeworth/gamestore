using FluentValidation;
using GameStore.Api.Models;
using GameStore.Api.Services;
using Microsoft.AspNetCore.Identity;

public class UserService
{
    private readonly UserRepository userRepository;
    private readonly IValidator<UserRegisterRequest> userRegisterValidator;
    private readonly IPasswordHasher<User> passwordHasher;

    public UserService(UserRepository userRepository,
    IValidator<UserRegisterRequest> userRegisterValidator,
    IPasswordHasher<User> passwordHasher)
    {
        this.userRepository = userRepository;
        this.userRegisterValidator = userRegisterValidator;
        this.passwordHasher = passwordHasher;
    }

    public async Task<OperationResult<User>> Register(UserRegisterRequest request)
    {
        var validation = await userRegisterValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return OperationResult<User>.Failure(validation.Errors[0].ErrorMessage,
                OperationErrorType.Validation);
        }

        var email = request.Email.Trim().ToLower();
        var exists = await userRepository.IsUserExists(email);

        if (exists)
        {
            return OperationResult<User>.Failure("User with such email already registred.",
             OperationErrorType.Validation);
        }

        var user = new User { Email = email, Role = "User" };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        await userRepository.CreateUser(user);

        return OperationResult<User>.Success(user);
    }
}