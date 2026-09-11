using Application.Features.Football.Seasons.Commands;
using Domain.Entities.Football.Competitions;
using FluentValidation;

namespace Application.Features.Football.Seasons.Validators;

public class ReplaceFootballSeasonContentBlocksCommandValidator
    : AbstractValidator<ReplaceFootballSeasonContentBlocksCommand>
{
    public ReplaceFootballSeasonContentBlocksCommandValidator()
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
                .MaximumLength(FootballSeasonContentBlock.TitleMaxLength)
                .WithMessage($"Block title cannot exceed {FootballSeasonContentBlock.TitleMaxLength} characters");

            item.RuleFor(block => block.ContentHtml)
                .MaximumLength(FootballSeasonContentBlock.ContentHtmlMaxLength)
                .WithMessage($"Block content cannot exceed {FootballSeasonContentBlock.ContentHtmlMaxLength} characters");
        });
    }
}
