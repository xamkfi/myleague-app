using Application.Features.Hockey.Teams.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Teams.Validators;

/// <summary>
/// Validator for <see cref="AddHockeyLineCommand"/>.
/// </summary>
public class AddHockeyLineCommandValidator : AbstractValidator<AddHockeyLineCommand>
{
    public AddHockeyLineCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LineNumber).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LineType).IsInEnum();
    }
}
