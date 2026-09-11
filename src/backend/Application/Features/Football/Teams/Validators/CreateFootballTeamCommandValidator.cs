using Application.Features.Football.Teams.Commands;
using Domain.Enums.Football;
using Domain.Enums.Common;
using FluentValidation;

namespace Application.Features.Football.Teams.Validators;

/// <summary>
/// Validator for CreateFootballTeamCommand
/// </summary>
public class CreateFootballTeamCommandValidator : AbstractValidator<CreateFootballTeamCommand>
{
    public CreateFootballTeamCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Team name is required")
            .MaximumLength(100).WithMessage("Team name cannot exceed 100 characters");

        RuleFor(x => x.ClubId)
            .NotEmpty().WithMessage("Club ID is required")
            .NotEqual(Guid.Empty).WithMessage("Club ID cannot be empty");

        // HomeArena, PrimaryJerseyColor and TeamCategory are all optional metadata. Tournament
        // imports often omit them — we only enforce maximum lengths and a valid enum value
        // when the caller actually supplies a value.
        RuleFor(x => x.HomeArena)
            .MaximumLength(200).WithMessage("Home arena cannot exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.HomeArena));

        RuleFor(x => x.PrimaryJerseyColor)
            .MaximumLength(50).WithMessage("Primary jersey color cannot exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PrimaryJerseyColor));

        RuleFor(x => x.TeamCategory!.Value)
            .IsInEnum().WithMessage("Invalid team category value")
            .When(x => x.TeamCategory.HasValue);

        RuleFor(x => x.SecondaryJerseyColor)
            .MaximumLength(50).WithMessage("Secondary jersey color cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.SecondaryJerseyColor));

        RuleFor(x => x.ShortName)
            .MaximumLength(4).WithMessage("Short name cannot exceed 4 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.ShortName));
    }
} 
