using FluentValidation;
using GameStore.Api.Models;

namespace GameStore.Api.Validation
{
    public class UserAddMoneyRequestValidator : AbstractValidator<UserAddMoneyRequest>
    {
        public UserAddMoneyRequestValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .PrecisionScale(MoneyHelper.Precision, MoneyHelper.Scale, true);
        }
    }
}