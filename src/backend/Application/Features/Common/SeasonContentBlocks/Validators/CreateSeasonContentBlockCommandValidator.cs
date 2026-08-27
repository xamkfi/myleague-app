using Application.Features.Common.SeasonContentBlocks.Commands;
using Domain.Enums.Common;
using FluentValidation;

namespace Application.Features.Common.SeasonContentBlocks.Validators;

/// <summary>
/// Validator for CreateSeasonContentBlockCommand
/// </summary>
public class CreateSeasonContentBlockCommandValidator : AbstractValidator<CreateSeasonContentBlockCommand>
{
    public CreateSeasonContentBlockCommandValidator()
    {
        RuleFor(x => x.Sport)
            .IsInEnum()
            .Must(sport => sport is SportsCategory.Floorball or SportsCategory.Football or SportsCategory.Icehockey)
            .WithMessage("Sport must be Floorball, Football, or Icehockey");

        RuleFor(x => x.CompetitionId)
            .NotEmpty().WithMessage("Competition ID is required");

        RuleFor(x => x.SeasonYear)
            .NotEmpty().WithMessage("Season year is required")
            .MaximumLength(32).WithMessage("Season year cannot exceed 32 characters");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.ContentHtml)
            .NotNull().WithMessage("Content HTML is required");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative");
    }
}
