using FluentValidation;
using GameStore.Api.Models;

namespace GameStore.Api.Validation;

public class AddCartItemRequestValidator : AbstractValidator<AddCartItemRequest>
{
    public AddCartItemRequestValidator()
    {
        RuleFor(x => x.GameId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
