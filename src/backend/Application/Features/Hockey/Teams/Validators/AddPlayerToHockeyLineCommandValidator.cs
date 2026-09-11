using Application.Features.Hockey.Teams.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Teams.Validators;

/// <summary>
/// Validator for <see cref="AddPlayerToHockeyLineCommand"/>.
/// </summary>
public class AddPlayerToHockeyLineCommandValidator : AbstractValidator<AddPlayerToHockeyLineCommand>
{
    public AddPlayerToHockeyLineCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.LineId).NotEmpty();
        RuleFor(x => x.TeamPlayerId).NotEmpty();
        RuleFor(x => x.Slot).IsInEnum();
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
    }
}
