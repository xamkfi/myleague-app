using Application.Commands.Clubs;
using FluentValidation;

namespace Application.Validators.Commands.Club;

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