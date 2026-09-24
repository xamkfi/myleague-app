using Application.Features.Hockey.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

/// <summary>
/// Validator for <see cref="DeleteHockeySeasonCommand"/>.
/// </summary>
public class DeleteHockeySeasonCommandValidator : AbstractValidator<DeleteHockeySeasonCommand>
{
    public DeleteHockeySeasonCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithMessage("Season ID is required");
    }
}
