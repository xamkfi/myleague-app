using Application.Features.Common.Organization.Clubs.Commands;
using FluentValidation;

namespace Application.Features.Common.Organization.Clubs.Validators;

/// <summary>
/// Validator for DeleteClubCommand
/// </summary>
public class DeleteClubCommandValidator : AbstractValidator<DeleteClubCommand>
{
    public DeleteClubCommandValidator()
    {
        RuleFor(x => x.ClubId)
            .NotEmpty().WithMessage("Club ID is required")
            .NotEqual(Guid.Empty).WithMessage("Club ID cannot be empty");
    }
} 
