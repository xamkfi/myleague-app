using System;
using Application.Features.Common.Organization.Users.Queries;
using FluentValidation;

namespace Application.Features.Common.Organization.Users.Validators
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
