using Application.Queries.Clubs;
using FluentValidation;

namespace Application.Validators.Queries.Club;

/// <summary>
/// Validator for GetClubsByNameQuery
/// </summary>
public class GetClubsByNameQueryValidator : AbstractValidator<GetClubsByNameQuery>
{
    public GetClubsByNameQueryValidator()
    {
        RuleFor(x => x.name)
            .NotEmpty().WithMessage("Name is required for search")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters long")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");
    }
}
