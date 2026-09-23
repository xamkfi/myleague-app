using Application.Features.Common.Organization.Persons.Queries;
using FluentValidation;

namespace Application.Features.Common.Organization.Persons.Validators
{
    /// <summary>
    /// Validator for SearchPersonByNameQuery
    /// </summary>
    public class SearchPersonByNameQueryValidator : AbstractValidator<SearchPersonByNameQuery>
    {
        public SearchPersonByNameQueryValidator()
        {
            RuleFor(x => x.name)
                .NotEmpty().WithMessage("Name is required for search")
                .MinimumLength(2).WithMessage("Name must be at least 2 characters long")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");
        }
    }
} 
