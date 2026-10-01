using Application.Features.Floorball.Matches.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Matches.Validators;

/// <summary>
/// Validates <see cref="AddFloorballScorekeeperToMatchCommand"/>.
/// </summary>
public class AddFloorballScorekeeperToMatchCommandValidator : AbstractValidator<AddFloorballScorekeeperToMatchCommand>
{
    public AddFloorballScorekeeperToMatchCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty().WithMessage("Match ID is required");
        RuleFor(x => x.PersonId).NotEmpty().WithMessage("Person ID is required");
    }
}

/// <summary>
/// Validates <see cref="RemoveFloorballScorekeeperFromMatchCommand"/>.
/// </summary>
public class RemoveFloorballScorekeeperFromMatchCommandValidator : AbstractValidator<RemoveFloorballScorekeeperFromMatchCommand>
{
    public RemoveFloorballScorekeeperFromMatchCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty().WithMessage("Match ID is required");
        RuleFor(x => x.PersonId).NotEmpty().WithMessage("Person ID is required");
    }
}
