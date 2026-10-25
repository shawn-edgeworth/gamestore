using FluentValidation;
using GameStore.Api.Models;

namespace GameStore.Api.Validation;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.OrderItems).NotEmpty();
        RuleForEach(x => x.OrderItems)
        .ChildRules(item =>
        {
            item.RuleFor(x => x.GameId).GreaterThan(0);
            item.RuleFor(x => x.Quantity).GreaterThan(0);
        });
    }
}
