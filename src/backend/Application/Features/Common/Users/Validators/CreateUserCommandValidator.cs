using Application.Features.Common.Users.Commands;
using FluentValidation;

namespace Application.Features.Common.Users.Validators;

/// <summary>
/// Validator for CreateUserCommand
/// </summary>
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email address is required")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters");

        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("Person ID is required")
            .NotEqual(Guid.Empty).WithMessage("Person ID cannot be empty");

        RuleForEach(x => x.TeamAssignments)
            .ChildRules(assignment =>
            {
                assignment.RuleFor(a => a.Sport)
                    .Must(s => string.Equals(s, "floorball", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(s, "football", StringComparison.OrdinalIgnoreCase))
                    .WithMessage("Sport must be 'floorball' or 'football'");

                assignment.RuleFor(a => a.TeamId)
                    .NotEqual(Guid.Empty).WithMessage("Team ID cannot be empty");
            })
            .When(x => x.TeamAssignments != null);
    }
}
