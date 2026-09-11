using System;
using Application.Features.Common.Users.Queries;
using FluentValidation;

namespace Application.Features.Common.Users.Validators
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
