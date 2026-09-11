using Application.Features.Football.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Football.Seasons.Validators;

public class DeleteFootballSeasonCommandValidator : AbstractValidator<DeleteFootballSeasonCommand>
{
    public DeleteFootballSeasonCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Season ID is required");
    }
}
