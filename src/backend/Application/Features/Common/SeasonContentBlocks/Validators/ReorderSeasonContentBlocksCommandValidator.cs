using Application.Features.Common.SeasonContentBlocks.Commands;
using FluentValidation;

namespace Application.Features.Common.SeasonContentBlocks.Validators;

/// <summary>
/// Validator for ReorderSeasonContentBlocksCommand
/// </summary>
public class ReorderSeasonContentBlocksCommandValidator : AbstractValidator<ReorderSeasonContentBlocksCommand>
{
    public ReorderSeasonContentBlocksCommandValidator()
    {
        RuleFor(x => x.OrderedIds)
            .NotEmpty().WithMessage("At least one block ID is required");

        RuleForEach(x => x.OrderedIds)
            .NotEmpty().WithMessage("Block ID cannot be empty");

        RuleFor(x => x.OrderedIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Block IDs must be unique");
    }
}
