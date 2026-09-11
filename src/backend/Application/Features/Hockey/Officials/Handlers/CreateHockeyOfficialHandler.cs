using Application.Common;
using Application.Features.Hockey.Officials.Commands;
using Application.Features.Hockey.Officials.DTOs;
using Application.Features.Hockey.Officials.Mappings;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Officials.Handlers;

/// <summary>
/// Handles creation of a hockey official profile.
/// </summary>
public class CreateHockeyOfficialHandler : IRequestHandler<CreateHockeyOfficialCommand, Result<HockeyOfficialDto>>
{
    private readonly IHockeyOfficialRepository _officialRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<CreateHockeyOfficialHandler> _logger;

    public CreateHockeyOfficialHandler(
        IHockeyOfficialRepository officialRepository,
        IPersonRepository personRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<CreateHockeyOfficialHandler> logger)
    {
        _officialRepository = officialRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyOfficialDto>> Handle(
        CreateHockeyOfficialCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            Person? person = await _personRepository.GetByIdAsync(request.PersonId);
            if (person is null)
            {
                return Result<HockeyOfficialDto>.NotFound("Person", request.PersonId);
            }

            HockeyOfficial? existing = await _officialRepository.GetByPersonIdAsync(request.PersonId);
            if (existing is not null)
            {
                return Result<HockeyOfficialDto>.Failure("This person is already a hockey official.");
            }

            HockeyOfficial official = new(
                request.PersonId,
                request.OfficialRole,
                request.OfficialNumber,
                DateTimeUtc.Normalize(request.LicenseIssueDate),
                DateTimeUtc.Normalize(request.LicenseExpiryDate));

            await _officialRepository.AddAsync(official);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created hockey official {OfficialId} for person {PersonId}",
                official.Id,
                request.PersonId);

            return Result<HockeyOfficialDto>.Success(HockeyOfficialMapper.ToDto(official));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid CreateHockeyOfficial for person {PersonId}", request.PersonId);
            return Result<HockeyOfficialDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed CreateHockeyOfficial for person {PersonId}", request.PersonId);
            return Result<HockeyOfficialDto>.Failure(
                "An error occurred while creating the hockey official.",
                ex.Flatten());
        }
    }
}
