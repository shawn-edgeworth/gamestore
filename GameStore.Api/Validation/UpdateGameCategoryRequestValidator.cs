using FluentValidation;
using GameStore.Api.Models;

namespace GameStore.Api.Validation;

public class UpdateGameCategoryRequestValidator : AbstractValidator<UpdateGameCategoryRequest>
{
     public UpdateGameCategoryRequestValidator()
    {
        RuleFor(x=>x.CategoryId).NotNull().GreaterThan(0);
    }
}