using FluentValidation;
using GameStore.Api.Helpers;
using GameStore.Api.Models;

namespace GameStore.Api.Validation;

public class UpdateGamePriceRequestValidator : AbstractValidator<UpdateGamePriceRequest>
{
     public UpdateGamePriceRequestValidator()
    {
        RuleFor(x=>x.Price)
            .NotNull()
            .GreaterThan(0)
            .PrecisionScale(MoneyHelper.Precision, MoneyHelper.Scale, true);
    }
}
