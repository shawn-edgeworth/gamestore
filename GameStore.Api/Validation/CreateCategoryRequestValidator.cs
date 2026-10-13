using FluentValidation;
using GameStore.Api.Models;

namespace GameStore.Api.Validation;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x=>x.Name).NotEmpty();
    }
}
