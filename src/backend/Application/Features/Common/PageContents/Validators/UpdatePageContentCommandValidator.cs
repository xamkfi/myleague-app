using FluentValidation;
using Application.Features.Common.PageContents.Commands;

namespace Application.Features.Common.PageContents.Validators
{
    /// <summary>
    /// Validator for UpdatePageContentCommand
    /// </summary>
    public class UpdatePageContentCommandValidator : AbstractValidator<UpdatePageContentCommand>
    {
        public UpdatePageContentCommandValidator()
        {
            RuleFor(x => x.Slug).NotEmpty().WithMessage("Page slug is required.");
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.").MaximumLength(200);
            RuleFor(x => x.ContentHtml).NotEmpty().WithMessage("ContentHtml is required.");
        }
    }
}
