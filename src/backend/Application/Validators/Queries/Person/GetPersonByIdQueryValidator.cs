using System;
using Application.Queries.Persons;
using FluentValidation;

namespace Application.Validators.Queries.Person
{
    /// <summary>
    /// Validator for GetPersonByIdQuery
    /// </summary>
    public class GetPersonByIdQueryValidator : AbstractValidator<GetPersonByIdQuery>
    {
        public GetPersonByIdQueryValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty().WithMessage("Person ID is required")
                .NotEqual(Guid.Empty).WithMessage("Person ID cannot be empty");
        }
    }
} 