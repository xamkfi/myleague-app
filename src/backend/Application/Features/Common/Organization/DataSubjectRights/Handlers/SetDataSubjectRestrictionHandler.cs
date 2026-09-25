using Application.Common;
using Application.Features.Common.Organization.DataSubjectRights.Commands;
using Application.Features.Common.Organization.DataSubjectRights.DTOs;
using Application.Features.Common.Organization.Deletion;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Organization.DataSubjectRights.Handlers;

/// <summary>
/// Restricts processing and suspends login, or lifts a restriction.
/// </summary>
public class SetDataSubjectRestrictionHandler : IRequestHandler<SetDataSubjectRestrictionCommand, Result<DataSubjectCopyDto>>
{
    private readonly IPersonRepository _personRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPersonDeletionGuard _deletionGuard;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetDataSubjectRestrictionHandler> _logger;

    public SetDataSubjectRestrictionHandler(
        IPersonRepository personRepository,
        IUserRepository userRepository,
        IPersonDeletionGuard deletionGuard,
        IUnitOfWork unitOfWork,
        ILogger<SetDataSubjectRestrictionHandler> logger)
    {
        _personRepository = personRepository;
        _userRepository = userRepository;
        _deletionGuard = deletionGuard;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<DataSubjectCopyDto>> Handle(SetDataSubjectRestrictionCommand request, CancellationToken cancellationToken)
    {
        if (request.IsRestricted)
        {
            Domain.Entities.Common.User? user = await _userRepository.GetByPersonIdAsync(request.PersonId);
            string? blockReason = await DataSubjectWorkflow.LastAdminBlockReasonAsync(user, _userRepository);
            if (blockReason is not null)
            {
                return Result<DataSubjectCopyDto>.Failure(blockReason);
            }
        }

        return await DataSubjectWorkflow.ChangeAsync(
            request.PersonId,
            _personRepository,
            _userRepository,
            _deletionGuard,
            _unitOfWork,
            _logger,
            cancellationToken,
            (person, user) =>
            {
                if (request.IsRestricted)
                {
                    person.RestrictProcessing();
                    user?.SuspendAccess();
                }
                else
                {
                    person.LiftProcessingRestriction();
                }

                return Task.CompletedTask;
            });
    }
}
