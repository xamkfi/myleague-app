using Application.Common;
using Application.Features.Common.Organization.DataSubjectRights.DTOs;
using Application.Features.Common.Organization.DataSubjectRights.Queries;
using Application.Features.Common.Organization.Deletion;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Organization.DataSubjectRights.Handlers;

/// <summary>
/// Returns a copy of the personal data held for one person.
/// </summary>
public class GetDataSubjectCopyHandler : IRequestHandler<GetDataSubjectCopyQuery, Result<DataSubjectCopyDto>>
{
    private readonly IPersonRepository _personRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPersonDeletionGuard _deletionGuard;
    private readonly ILogger<GetDataSubjectCopyHandler> _logger;

    public GetDataSubjectCopyHandler(
        IPersonRepository personRepository,
        IUserRepository userRepository,
        IPersonDeletionGuard deletionGuard,
        ILogger<GetDataSubjectCopyHandler> logger)
    {
        _personRepository = personRepository;
        _userRepository = userRepository;
        _deletionGuard = deletionGuard;
        _logger = logger;
    }

    public Task<Result<DataSubjectCopyDto>> Handle(GetDataSubjectCopyQuery request, CancellationToken cancellationToken)
    {
        return DataSubjectWorkflow.ReadAsync(
            request.PersonId,
            _personRepository,
            _userRepository,
            _deletionGuard,
            _logger,
            cancellationToken);
    }
}
