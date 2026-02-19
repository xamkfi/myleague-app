using Application.Features.Common.News.Commands;
using FluentValidation;

namespace Application.Features.Common.News.Validators;

/// <summary>
/// Validator for RestoreNewsArticleCommand
/// </summary>
public class RestoreNewsArticleCommandValidator : AbstractValidator<RestoreNewsArticleCommand>
{
    public RestoreNewsArticleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("News article ID is required")
            .NotEqual(Guid.Empty).WithMessage("News article ID cannot be empty");
    }
} 
