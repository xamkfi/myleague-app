using Application.Features.Football.Matches.Commands;
using FluentValidation;

namespace Application.Features.Football.Matches.Validators;

/// <summary>
/// Validates <see cref="AddFootballScorekeeperToMatchCommand"/>.
/// </summary>
public class AddFootballScorekeeperToMatchCommandValidator : AbstractValidator<AddFootballScorekeeperToMatchCommand>
{
    public AddFootballScorekeeperToMatchCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty().WithMessage("Match ID is required");
        RuleFor(x => x.PersonId).NotEmpty().WithMessage("Person ID is required");
    }
}

/// <summary>
/// Validates <see cref="RemoveFootballScorekeeperFromMatchCommand"/>.
/// </summary>
public class RemoveFootballScorekeeperFromMatchCommandValidator : AbstractValidator<RemoveFootballScorekeeperFromMatchCommand>
{
    public RemoveFootballScorekeeperFromMatchCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty().WithMessage("Match ID is required");
        RuleFor(x => x.PersonId).NotEmpty().WithMessage("Person ID is required");
    }
}
