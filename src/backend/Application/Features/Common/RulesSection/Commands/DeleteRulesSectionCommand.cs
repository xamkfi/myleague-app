using Application.Common;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.RulesSection.Commands;

/// <summary>
/// Command for deleting a rules section
/// </summary>
public record DeleteRulesSectionCommand(Guid Id) : IRequest<Result<bool>>;

/// <summary>
/// Handler for deleting a rules section
/// </summary>
public class DeleteRulesSectionCommandHandler
    : IRequestHandler<DeleteRulesSectionCommand, Result<bool>>
{
    private readonly IRulesSectionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the DeleteRulesSectionCommandHandler class
    /// </summary>
    /// <param name="repository">The rules section repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    public DeleteRulesSectionCommandHandler(
        IRulesSectionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the DeleteRulesSectionCommand request
    /// </summary>
    /// <param name="request">The command containing the section ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, wrapped in a Result</returns>
    public async Task<Result<bool>> Handle(
        DeleteRulesSectionCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Common.RulesSection? entity = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (entity == null)
        {
            return Result<bool>.Failure($"Rules section with ID '{request.Id}' not found.");
        }

        bool hasChildren = await _repository.HasChildSectionsAsync(request.Id, cancellationToken);

        if (hasChildren)
        {
            return Result<bool>.Failure("Cannot delete a section that has child sections.");
        }

        if (!string.IsNullOrWhiteSpace(entity.ContentHtml))
        {
            return Result<bool>.Failure("Cannot delete a section that contains rules. Remove all rules first.");
        }

        await _repository.RemoveAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
