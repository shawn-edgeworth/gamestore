using FluentValidation;
using GameStore.Api.Data;
using GameStore.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace GameStore.Api.Services;

public class UserService
{
    private readonly UserRepository userRepository;
    private readonly IValidator<UserRegisterRequest> userRegisterValidator;
    private readonly IValidator<UserLoginRequest> userLoginValidator;
    private readonly IPasswordHasher<User> passwordHasher;
    private readonly IValidator<UserAddMoneyRequest> userAddMoneyValidator;
    private readonly JwtTokenService jwtTokenService;

    public UserService(UserRepository userRepository,
    IValidator<UserRegisterRequest> userRegisterValidator,
    IValidator<UserLoginRequest> userLoginValidator,
    IPasswordHasher<User> passwordHasher,
    IValidator<UserAddMoneyRequest> userAddMoneyValidator,
    JwtTokenService jwtTokenService)
    {
        this.userRepository = userRepository;
        this.userRegisterValidator = userRegisterValidator;
        this.userLoginValidator = userLoginValidator;
        this.passwordHasher = passwordHasher;
        this.userAddMoneyValidator = userAddMoneyValidator;
        this.jwtTokenService = jwtTokenService;
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

    public async Task<OperationResult<UserLoginResponse>> Login(UserLoginRequest request)
    {
        var validation = await userLoginValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return OperationResult<UserLoginResponse>.Failure(validation.Errors[0].ErrorMessage,
                OperationErrorType.Validation);
        }

        var email = request.Email.Trim().ToLower();
        var user = await userRepository.GetUserByEmail(email);

        if (user is null)
        {
            return OperationResult<UserLoginResponse>.Failure("User is not found",
               OperationErrorType.Authorization);
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (verificationResult is PasswordVerificationResult.Failed)
        {
            return OperationResult<UserLoginResponse>.Failure("Incorrect password.",
                OperationErrorType.Authorization);
        }

        var token = jwtTokenService.CreateToken(user);
        return OperationResult<UserLoginResponse>.Success(
            new UserLoginResponse
            {
                Token = token,
                Email = user.Email,
                Role = user.Role
            }
        );
    }

    public async Task<OperationResult<User>> AddMoney(int userId, UserAddMoneyRequest request)
    {
        var validation = await userAddMoneyValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return OperationResult<User>.Failure(validation.Errors[0].ErrorMessage,
                OperationErrorType.Validation);
        }

        var user = await userRepository.AddMoney(userId, request.Amount);

        if (user is null)
        {
            return OperationResult<User>.Failure("User is not found",
               OperationErrorType.NotFound);
        }

        return OperationResult<User>.Success(user);
    }
}