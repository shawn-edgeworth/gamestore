using FluentValidation;
using GameStore.Api.Models;

namespace GameStore.Api.Validation;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(16);
    }
}
