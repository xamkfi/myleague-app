using Application.Features.Common.News.Commands;
using FluentValidation;

namespace Application.Features.Common.News.Validators;

/// <summary>
/// Validator for RemoveNewsArticleTagCommand
/// </summary>
public class RemoveNewsArticleTagCommandValidator : AbstractValidator<RemoveNewsArticleTagCommand>
{
    public RemoveNewsArticleTagCommandValidator()
    {
        RuleFor(x => x.NewsId)
            .NotEmpty().WithMessage("News article ID is required")
            .NotEqual(Guid.Empty).WithMessage("News article ID cannot be empty");

        RuleFor(x => x.Tag)
            .NotEmpty().WithMessage("Tag is required")
            .MaximumLength(50).WithMessage("Tag cannot exceed 50 characters");
    }
} 
