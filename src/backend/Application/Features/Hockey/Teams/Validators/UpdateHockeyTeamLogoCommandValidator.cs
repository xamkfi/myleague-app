using Application.Features.Hockey.Teams.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Teams.Validators;

/// <summary>
/// Validator for <see cref="UpdateHockeyTeamLogoCommand"/>.
/// </summary>
public class UpdateHockeyTeamLogoCommandValidator : AbstractValidator<UpdateHockeyTeamLogoCommand>
{
    public UpdateHockeyTeamLogoCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty().WithMessage("Team id is required.");
        RuleFor(x => x.LogoUrl)
            .Must(url => string.IsNullOrWhiteSpace(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Logo url must be an absolute URI when provided.");
    }
}
