using Application.Common;
using Application.DTOs.Common;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.RulesSection.Commands;

/// <summary>
/// Command for updating a rule within a rules section
/// </summary>
public record UpdateRulesSectionRuleCommand(
    Guid SectionId,
    string RuleId,
    string RuleHtml,
    string? LastModifiedBy
) : IRequest<Result<RulesSectionDto>>;

/// <summary>
/// Handler for updating a rule within a rules section
/// </summary>
public class UpdateRulesSectionRuleCommandHandler
    : IRequestHandler<UpdateRulesSectionRuleCommand, Result<RulesSectionDto>>
{
    private readonly IRulesSectionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the UpdateRulesSectionRuleCommandHandler class
    /// </summary>
    /// <param name="repository">The rules section repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    public UpdateRulesSectionRuleCommandHandler(
        IRulesSectionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the UpdateRulesSectionRuleCommand request
    /// </summary>
    /// <param name="request">The command containing the rule ID and updated HTML</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated rules section as a DTO wrapped in a Result</returns>
    public async Task<Result<RulesSectionDto>> Handle(
        UpdateRulesSectionRuleCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Common.RulesSection? entity = await _repository.GetByIdAsync(
            request.SectionId,
            cancellationToken);

        if (entity == null)
        {
            return Result<RulesSectionDto>.Failure($"Rules section with ID '{request.SectionId}' not found.");
        }

        try
        {
            string updatedHtml = RulesHtmlHelper.UpdateRule(
                entity.ContentHtml,
                request.RuleId,
                request.RuleHtml);

            entity.UpdateContentHtml(updatedHtml, request.LastModifiedBy);
            await _repository.UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<RulesSectionDto>.Success(RulesSectionMapper.ToDto(entity));
        }
        catch (InvalidOperationException ex)
        {
            return Result<RulesSectionDto>.Failure(ex.Message);
        }
    }
}
