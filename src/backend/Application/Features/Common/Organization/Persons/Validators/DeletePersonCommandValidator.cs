using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.Organization.Clubs.Commands;
using Application.Features.Common.Organization.Persons.Commands;
using FluentValidation;

namespace Application.Features.Common.Organization.Persons.Validators
{
    /// <summary>
    /// Validator for DeletePersonCommand
    /// </summary>
    public class DeletePersonCommandValidator : AbstractValidator<DeletePersonCommand>
    {
        public DeletePersonCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Person ID is required")
                .NotEqual(Guid.Empty).WithMessage("Person ID cannot be empty");
        }
    }
}
