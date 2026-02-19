using Application.Features.Common.News.Commands;
using FluentValidation;

namespace Application.Features.Common.News.Validators;

/// <summary>
/// Validator for ArchiveNewsArticleCommand
/// </summary>
public class ArchiveNewsArticleCommandValidator : AbstractValidator<ArchiveNewsArticleCommand>
{
    public ArchiveNewsArticleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("News article ID is required")
            .NotEqual(Guid.Empty).WithMessage("News article ID cannot be empty");
    }
} 
