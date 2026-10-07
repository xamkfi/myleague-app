using Application.Common;
using Application.Features.Hockey.Seasons.Commands;
using Domain.Services.Common;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="CreateHockeySeasonCommand"/>.
/// </summary>
public class CreateHockeySeasonCommandValidator : AbstractValidator<CreateHockeySeasonCommand>
{
    public CreateHockeySeasonCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Season name is required.")
            .MaximumLength(100).WithMessage("Season name cannot exceed 100 characters.");

        RuleFor(x => x.StartDate)
            .NotEqual(default(DateTime)).WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEqual(default(DateTime)).WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");

        RuleFor(x => x.SeasonCode)
            .MaximumLength(50).WithMessage("Season code cannot exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.SeasonCode));

        RuleFor(x => x.TeamCategory).IsInEnum();
        RuleFor(x => x.LogoUrl)
            .Must(CompetitionLogoUrl.IsValidOptional)
            .WithMessage("Logo url must be an http or https address under 500 characters");

        RuleFor(x => x.TeamsAdvancing)
            .GreaterThanOrEqualTo(0).WithMessage("Teams advancing cannot be negative.");

        RuleFor(x => x.RankingCriteria)
            .Must(StandingSortCriteria.IsValid)
            .WithMessage("Ranking criteria must be a non-empty list of unique values.");

        RuleFor(x => x.RegulationWinPoints).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OvertimeWinPoints).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ShootoutWinPoints).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OvertimeLossPoints).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ShootoutLossPoints).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TiePoints).GreaterThanOrEqualTo(0);
    }
}
