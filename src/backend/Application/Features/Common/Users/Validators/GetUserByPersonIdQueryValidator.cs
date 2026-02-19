using System;
using Application.Queries.Users;
using FluentValidation;

namespace Application.Validators.Queries.Users
{
    /// <summary>
    /// Validator for GetUserByPersonIdQuery
    /// </summary>
    public class GetUserByPersonIdQueryValidator : AbstractValidator<GetUserByPersonIdQuery>
    {
        public GetUserByPersonIdQueryValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty().WithMessage("Person ID is required")
                .NotEqual(Guid.Empty).WithMessage("Person ID cannot be empty");
        }
    }
} 