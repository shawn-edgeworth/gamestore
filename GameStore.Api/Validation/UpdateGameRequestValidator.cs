using FluentValidation;
using GameStore.Api.Helpers;
using GameStore.Api.Models;

namespace GameStore.Api.Validation;

public class UpdateGameRequestValidator : AbstractValidator<UpdateGameRequest>
{
    public UpdateGameRequestValidator()
    {
        RuleFor(x => x.Name).NotNull().NotEmpty();
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .PrecisionScale(MoneyHelper.Precision, MoneyHelper.Scale, true);
        RuleFor(x => x.CategoryId).GreaterThanOrEqualTo(0);
    }
}
