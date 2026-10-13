using FluentValidation;
using GameStore.Api.Models;

namespace GameStore.Api.Validation;

public class UpdateGamePriceRequestValidator : AbstractValidator<UpdateGamePriceRequest>
{
     public UpdateGamePriceRequestValidator()
    {
        RuleFor(x=>x.Price).NotNull().GreaterThan(0);
    }
}
