using Application.Features.Hockey.Seasons.Commands;
using Domain.Entities.Hockey.Competitions;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

public class ReplaceHockeySeasonContentBlocksCommandValidator
    : AbstractValidator<ReplaceHockeySeasonContentBlocksCommand>
{
    public ReplaceHockeySeasonContentBlocksCommandValidator()
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
                .MaximumLength(HockeySeasonContentBlock.TitleMaxLength)
                .WithMessage($"Block title cannot exceed {HockeySeasonContentBlock.TitleMaxLength} characters");

            item.RuleFor(block => block.ContentHtml)
                .MaximumLength(HockeySeasonContentBlock.ContentHtmlMaxLength)
                .WithMessage($"Block content cannot exceed {HockeySeasonContentBlock.ContentHtmlMaxLength} characters");
        });
    }
}
