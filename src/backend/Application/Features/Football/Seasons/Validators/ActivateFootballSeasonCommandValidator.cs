using Application.Features.Football.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Football.Seasons.Validators;

public class ActivateFootballSeasonCommandValidator : AbstractValidator<ActivateFootballSeasonCommand>
{
    public ActivateFootballSeasonCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Season ID is required");
    }
}
