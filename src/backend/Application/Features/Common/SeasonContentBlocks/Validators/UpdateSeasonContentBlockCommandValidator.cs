using Application.Features.Common.SeasonContentBlocks.Commands;
using FluentValidation;

namespace Application.Features.Common.SeasonContentBlocks.Validators;

/// <summary>
/// Validator for UpdateSeasonContentBlockCommand
/// </summary>
public class UpdateSeasonContentBlockCommandValidator : AbstractValidator<UpdateSeasonContentBlockCommand>
{
    public UpdateSeasonContentBlockCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Block ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.ContentHtml)
            .NotNull().WithMessage("Content HTML is required");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative");
    }
}
