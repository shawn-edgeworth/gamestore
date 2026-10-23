using FluentValidation;
using GameStore.Api.Models;

namespace GameStore.Api.Validation
{
    public class UserRegisterRequestValidator : AbstractValidator<UserRegisterRequest>
    {
        public UserRegisterRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        }
    }
}