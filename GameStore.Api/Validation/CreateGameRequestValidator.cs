using FluentValidation;
using GameStore.Api.Models;

namespace GameStore.Api.Validation;

public class CreateGameRequestValidator : AbstractValidator<CreateGameRequest>
{
    public CreateGameRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .PrecisionScale(MoneyHelper.Precision, MoneyHelper.Scale, true);
        RuleFor(x => x.CategoryId).GreaterThan(0);
    }
}
