using Application.Features.Common.Clubs.Queries;
using FluentValidation;

namespace Application.Features.Common.Clubs.Validators;

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
