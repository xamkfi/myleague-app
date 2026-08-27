using Application.Features.Common.SeasonContentBlocks.Queries;
using FluentValidation;

namespace Application.Features.Common.SeasonContentBlocks.Validators;

/// <summary>
/// Validator for GetSeasonContentBlockByIdQuery
/// </summary>
public class GetSeasonContentBlockByIdQueryValidator : AbstractValidator<GetSeasonContentBlockByIdQuery>
{
    public GetSeasonContentBlockByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Block ID is required");
    }
}
