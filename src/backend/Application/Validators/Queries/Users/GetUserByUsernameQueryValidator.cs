using System;
using Application.Queries.Users;
using FluentValidation;

namespace Application.Validators.Queries.Users
{
    /// <summary>
    /// Validator for GetUserByUsernameQuery
    /// </summary>
    public class GetUserByUsernameQueryValidator : AbstractValidator<GetUserByUsernameQuery>
    {
        public GetUserByUsernameQueryValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MaximumLength(100).WithMessage("Username cannot exceed 100 characters");
        }
    }
} 