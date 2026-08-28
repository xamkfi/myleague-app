using Application.Features.Floorball.Seasons.Commands;
using Domain.Entities.Floorball;
using FluentValidation;

namespace Application.Features.Floorball.Seasons.Validators;

public class ReplaceFloorballSeasonContentBlocksCommandValidator
    : AbstractValidator<ReplaceFloorballSeasonContentBlocksCommand>
{
    public ReplaceFloorballSeasonContentBlocksCommandValidator()
    {
        RuleFor(command => command.SeasonId)
            .NotEmpty()
            .WithMessage("Season ID is required");

        RuleFor(command => command.Items)
            .NotNull()
            .WithMessage("Content blocks are required");

        RuleForEach(command => command.Items).ChildRules(item =>
        {
            item.RuleFor(block => block.Title)
                .NotEmpty()
                .WithMessage("Block title is required")
                .MaximumLength(FloorballSeasonContentBlock.TitleMaxLength)
                .WithMessage($"Block title cannot exceed {FloorballSeasonContentBlock.TitleMaxLength} characters");

            item.RuleFor(block => block.ContentHtml)
                .MaximumLength(FloorballSeasonContentBlock.ContentHtmlMaxLength)
                .WithMessage($"Block content cannot exceed {FloorballSeasonContentBlock.ContentHtmlMaxLength} characters");
        });
    }
}
