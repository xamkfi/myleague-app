using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Commands.Clubs;
using Application.Commands.Persons;
using FluentValidation;

namespace Application.Validators.Commands.Person
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
