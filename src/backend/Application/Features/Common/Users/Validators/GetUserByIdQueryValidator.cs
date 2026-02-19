using System;
using Application.Queries.Users;
using FluentValidation;

namespace Application.Validators.Queries.Users
{
    /// <summary>
    /// Validator for GetUserByIdQuery
    /// </summary>
    public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
    {
        public GetUserByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("User ID is required")
                .NotEqual(Guid.Empty).WithMessage("User ID cannot be empty");
        }
    }
} 