using Application.Queries.Persons;
using FluentValidation;

namespace Application.Validators.Queries.Person
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
