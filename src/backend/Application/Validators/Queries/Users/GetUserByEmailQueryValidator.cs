using Application.Queries.Users;
using FluentValidation;

namespace Application.Validators.Queries.Users;

/// <summary>
/// Validator for GetUserByEmailQuery
/// </summary>
public class GetUserByEmailQueryValidator : AbstractValidator<GetUserByEmailQuery>
{
    public GetUserByEmailQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email address is required")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters");
    }
}
