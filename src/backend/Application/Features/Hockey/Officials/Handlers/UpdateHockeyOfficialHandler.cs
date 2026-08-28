using Application.Common;
using Application.Features.Hockey.Officials.Commands;
using Application.Features.Hockey.Officials.DTOs;
using Application.Features.Hockey.Officials.Mappings;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Officials.Handlers;

/// <summary>
/// Handles updating a hockey official profile.
/// </summary>
public class UpdateHockeyOfficialHandler : IRequestHandler<UpdateHockeyOfficialCommand, Result<HockeyOfficialDto>>
{
    private readonly IHockeyOfficialRepository _officialRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateHockeyOfficialHandler> _logger;

    public UpdateHockeyOfficialHandler(
        IHockeyOfficialRepository officialRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<UpdateHockeyOfficialHandler> logger)
    {
        _officialRepository = officialRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyOfficialDto>> Handle(
        UpdateHockeyOfficialCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyOfficial? official = await _officialRepository.GetByIdAsync(request.OfficialId);
            if (official is null)
            {
                return Result<HockeyOfficialDto>.NotFound("HockeyOfficial", request.OfficialId);
            }

            official.UpdateOfficialRole(request.OfficialRole);
            official.UpdateOfficialNumber(request.OfficialNumber);
            official.UpdateLicenseDates(
                DateTimeUtc.Normalize(request.LicenseIssueDate),
                DateTimeUtc.Normalize(request.LicenseExpiryDate));
            official.UpdateActiveStatus(request.IsActive);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated hockey official {OfficialId}", request.OfficialId);
            return Result<HockeyOfficialDto>.Success(HockeyOfficialMapper.ToDto(official));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid UpdateHockeyOfficial for {OfficialId}", request.OfficialId);
            return Result<HockeyOfficialDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed UpdateHockeyOfficial for {OfficialId}", request.OfficialId);
            return Result<HockeyOfficialDto>.Failure(
                "An error occurred while updating the hockey official.",
                ex.Flatten());
        }
    }
}
