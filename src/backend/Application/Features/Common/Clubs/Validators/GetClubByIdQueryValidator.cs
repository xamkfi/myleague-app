using Application.Queries.Clubs;
using FluentValidation;

namespace Application.Validators.Queries.Club;

/// <summary>
/// Validator for GetClubByIdQuery
/// </summary>
public class GetClubByIdQueryValidator : AbstractValidator<GetClubByIdQuery>
{
    public GetClubByIdQueryValidator()
    {
        RuleFor(x => x.ClubId)
            .NotEmpty().WithMessage("Club ID is required")
            .NotEqual(Guid.Empty).WithMessage("Club ID cannot be empty");
    }
} 
