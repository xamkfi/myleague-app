using Application.Features.Common.Organization.Persons.Queries;
using FluentValidation;

namespace Application.Features.Common.Organization.Persons.Validators;

/// <summary>
/// Validator for <see cref="GetPersonPlayerSportsQuery"/>.
/// </summary>
public class GetPersonPlayerSportsQueryValidator : AbstractValidator<GetPersonPlayerSportsQuery>
{
    public GetPersonPlayerSportsQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Person or player ID is required")
            .NotEqual(Guid.Empty).WithMessage("Person or player ID cannot be empty");
    }
}
