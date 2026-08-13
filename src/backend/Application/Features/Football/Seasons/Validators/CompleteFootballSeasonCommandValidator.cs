using Application.Features.Football.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Football.Seasons.Validators;

public class CompleteFootballSeasonCommandValidator : AbstractValidator<CompleteFootballSeasonCommand>
{
    public CompleteFootballSeasonCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Season ID is required");
    }
}
