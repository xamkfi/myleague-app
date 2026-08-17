using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class UpdateHockeyMatchVenueCommandValidator : AbstractValidator<UpdateHockeyMatchVenueCommand>
{
    public UpdateHockeyMatchVenueCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.Venue).MaximumLength(200).When(x => x.Venue is not null);
    }
}
