using Application.Common;
using Application.Features.Common.Organization.DataSubjectRights.Commands;
using Application.Features.Common.Organization.DataSubjectRights.DTOs;
using Application.Features.Common.Organization.DataSubjectRights.Mappings;
using Application.Features.Common.Organization.Deletion;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Organization.DataSubjectRights.Handlers;

/// <summary>
/// Anonymizes identity data, deactivates the account, and revokes sessions.
/// </summary>
public class EraseDataSubjectHandler : IRequestHandler<EraseDataSubjectCommand, Result<DataSubjectCopyDto>>
{
    private readonly IPersonRepository _personRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPersonDeletionGuard _deletionGuard;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EraseDataSubjectHandler> _logger;

    public EraseDataSubjectHandler(
        IPersonRepository personRepository,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPersonDeletionGuard deletionGuard,
        IUnitOfWork unitOfWork,
        ILogger<EraseDataSubjectHandler> logger)
    {
        _personRepository = personRepository;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _deletionGuard = deletionGuard;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<DataSubjectCopyDto>> Handle(EraseDataSubjectCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Common.User? user = await _userRepository.GetByPersonIdAsync(request.PersonId);
        string? blockReason = await DataSubjectWorkflow.LastAdminBlockReasonAsync(user, _userRepository);
        if (blockReason is not null)
        {
            return Result<DataSubjectCopyDto>.Failure(blockReason);
        }

        return await DataSubjectWorkflow.ChangeAsync(
            request.PersonId,
            _personRepository,
            _userRepository,
            _deletionGuard,
            _unitOfWork,
            _logger,
            cancellationToken,
            async (person, account) =>
            {
                person.Anonymize(DateTime.UtcNow);
                if (account is not null)
                {
                    account.EraseAccount(DataSubjectMapper.ErasedAccountEmail(person.Id));
                    await _refreshTokenRepository.RevokeAllByUserIdAsync(account.Id);
                }
            });
    }
}
