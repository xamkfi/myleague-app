using Application.Commands.NewsArticles;
using FluentValidation;

namespace Application.Validators.Commands.NewsArticles;

/// <summary>
/// Validator for AddNewsArticleTagCommand
/// </summary>
public class AddNewsArticleTagCommandValidator : AbstractValidator<AddNewsArticleTagCommand>
{
    public AddNewsArticleTagCommandValidator()
    {
        RuleFor(x => x.NewsId)
            .NotEmpty().WithMessage("News article ID is required")
            .NotEqual(Guid.Empty).WithMessage("News article ID cannot be empty");

        RuleFor(x => x.Tag)
            .NotEmpty().WithMessage("Tag is required")
            .MaximumLength(50).WithMessage("Tag cannot exceed 50 characters");
    }
} 