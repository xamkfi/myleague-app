using Application.Features.Common.Organization.DataSubjectRights.Commands;
using FluentValidation;

namespace Application.Features.Common.Organization.DataSubjectRights.Validators;

/// <summary>
/// Validator for <see cref="SetDataSubjectObjectionCommand"/>.
/// </summary>
public class SetDataSubjectObjectionCommandValidator : AbstractValidator<SetDataSubjectObjectionCommand>
{
    public SetDataSubjectObjectionCommandValidator()
    {
        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("Person ID is required");
    }
}
