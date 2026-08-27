using Application.Features.Common.SeasonContentBlocks.Commands;
using FluentValidation;

namespace Application.Features.Common.SeasonContentBlocks.Validators;

/// <summary>
/// Validator for DeleteSeasonContentBlockCommand
/// </summary>
public class DeleteSeasonContentBlockCommandValidator : AbstractValidator<DeleteSeasonContentBlockCommand>
{
    public DeleteSeasonContentBlockCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Block ID is required");
    }
}
