using Application.Features.Common.Persons.Queries;
using FluentValidation;

namespace Application.Features.Common.Persons.Validators
{
    /// <summary>
    /// Validator for GetPersonByEmailQuery
    /// </summary>
    public class GetPersonByEmailQueryValidator : AbstractValidator<GetPersonByEmailQuery>
    {
        public GetPersonByEmailQueryValidator()
        {
            RuleFor(x => x.email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(200).WithMessage("Email cannot exceed 200 characters");
        }
    }
} 
