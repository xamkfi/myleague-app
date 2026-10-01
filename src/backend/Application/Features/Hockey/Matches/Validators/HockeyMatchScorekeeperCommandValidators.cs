using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

/// <summary>
/// Validates <see cref="AddHockeyMatchScorekeeperCommand"/>.
/// </summary>
public class AddHockeyMatchScorekeeperCommandValidator : AbstractValidator<AddHockeyMatchScorekeeperCommand>
{
    public AddHockeyMatchScorekeeperCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty().WithMessage("Match ID is required");
        RuleFor(x => x.PersonId).NotEmpty().WithMessage("Person ID is required");
    }
}

/// <summary>
/// Validates <see cref="RemoveHockeyMatchScorekeeperCommand"/>.
/// </summary>
public class RemoveHockeyMatchScorekeeperCommandValidator : AbstractValidator<RemoveHockeyMatchScorekeeperCommand>
{
    public RemoveHockeyMatchScorekeeperCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty().WithMessage("Match ID is required");
        RuleFor(x => x.PersonId).NotEmpty().WithMessage("Person ID is required");
    }
}
