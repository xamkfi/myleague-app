using Application.Features.Hockey.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="PublishHockeySeasonCommand"/>.
/// </summary>
public class PublishHockeySeasonCommandValidator : AbstractValidator<PublishHockeySeasonCommand>
{
    public PublishHockeySeasonCommandValidator()
    {
        RuleFor(x => x.SeasonId).NotEmpty().WithMessage("Season id is required.");
    }
}
