using Application.Common;
using Application.Features.Common.Organization.DataSubjectRights.Commands;
using Application.Features.Common.Organization.DataSubjectRights.DTOs;
using Application.Features.Common.Organization.Deletion;
using Application.Features.Common.Organization.Persons.Mappings;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Organization.DataSubjectRights.Handlers;

/// <summary>
/// Corrects identity and account email the person supplied.
/// </summary>
public class RectifyDataSubjectHandler : IRequestHandler<RectifyDataSubjectCommand, Result<DataSubjectCopyDto>>
{
    private readonly IPersonRepository _personRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPersonDeletionGuard _deletionGuard;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RectifyDataSubjectHandler> _logger;

    public RectifyDataSubjectHandler(
        IPersonRepository personRepository,
        IUserRepository userRepository,
        IPersonDeletionGuard deletionGuard,
        IUnitOfWork unitOfWork,
        ILogger<RectifyDataSubjectHandler> logger)
    {
        _personRepository = personRepository;
        _userRepository = userRepository;
        _deletionGuard = deletionGuard;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<Result<DataSubjectCopyDto>> Handle(RectifyDataSubjectCommand request, CancellationToken cancellationToken)
    {
        return DataSubjectWorkflow.ChangeAsync(
            request.PersonId,
            _personRepository,
            _userRepository,
            _deletionGuard,
            _unitOfWork,
            _logger,
            cancellationToken,
            (person, user) =>
            {
                person.Rectify(
                    request.FirstName,
                    request.LastName,
                    request.BirthDate,
                    PersonMapper.ToAddress(request.Address),
                    PersonMapper.ToContactInfo(request.ContactInfo));

                if (user is not null && !string.IsNullOrWhiteSpace(request.AccountEmail))
                {
                    user.ChangeEmail(request.AccountEmail);
                }

                return Task.CompletedTask;
            });
    }
}
