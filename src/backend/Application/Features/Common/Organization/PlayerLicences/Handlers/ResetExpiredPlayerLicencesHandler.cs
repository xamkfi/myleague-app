using Application.Common;
using Application.Features.Common.Organization.PlayerLicences.Commands;
using Application.Features.Common.Organization.PlayerLicences.DTOs;
using Application.Interfaces.Common;
using Domain.Common;
using Domain.Repositories.Common;
using SiteSettingsEntity = Domain.Entities.Common.SiteSettings;
using Domain.Repositories.Floorball;
using Domain.Repositories.Football;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Organization.PlayerLicences.Handlers;

public class ResetExpiredPlayerLicencesHandler
    : IRequestHandler<ResetExpiredPlayerLicencesCommand, Result<PlayerLicenceResetResultDto>>
{
    private readonly ISiteSettingsRepository _siteSettingsRepository;
    private readonly ISiteSettingsProvider _siteSettingsProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFloorballTeamRepository _floorballTeamRepository;
    private readonly IFootballTeamRepository _footballTeamRepository;
    private readonly IHockeyTeamRepository _hockeyTeamRepository;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ResetExpiredPlayerLicencesHandler> _logger;

    public ResetExpiredPlayerLicencesHandler(
        ISiteSettingsRepository siteSettingsRepository,
        ISiteSettingsProvider siteSettingsProvider,
        IUnitOfWork unitOfWork,
        IFloorballTeamRepository floorballTeamRepository,
        IFootballTeamRepository footballTeamRepository,
        IHockeyTeamRepository hockeyTeamRepository,
        TimeProvider timeProvider,
        ILogger<ResetExpiredPlayerLicencesHandler> logger)
    {
        _siteSettingsRepository = siteSettingsRepository;
        _siteSettingsProvider = siteSettingsProvider;
        _unitOfWork = unitOfWork;
        _floorballTeamRepository = floorballTeamRepository;
        _footballTeamRepository = footballTeamRepository;
        _hockeyTeamRepository = hockeyTeamRepository;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Result<PlayerLicenceResetResultDto>> Handle(
        ResetExpiredPlayerLicencesCommand request,
        CancellationToken cancellationToken)
    {
        DateOnly today = PlayerLicenceCalendar.TodayInHelsinki(_timeProvider.GetUtcNow().UtcDateTime);
        SiteSettingsEntity? settings = await _siteSettingsRepository.GetAsync(cancellationToken);

        int month = settings?.PlayerLicenceResetMonth ?? SiteSettingsEntity.PlayerLicenceResetMonthDefault;
        int day = settings?.PlayerLicenceResetDay ?? SiteSettingsEntity.PlayerLicenceResetDayDefault;
        int? lastResetYear = settings?.LastPlayerLicenceResetYear;
        int currentCutoffYear = PlayerLicenceCalendar.CurrentCutoffYear(today, month, day);

        if (!request.Force && lastResetYear is null)
        {
            await EnsureSettingsInitializedAsync(settings, currentCutoffYear, cancellationToken);
            return Result<PlayerLicenceResetResultDto>.Success(
                new PlayerLicenceResetResultDto(false, currentCutoffYear, 0, 0, 0));
        }

        if (!request.Force && !PlayerLicenceCalendar.IsAutomaticResetDue(today, month, day, lastResetYear))
        {
            return Result<PlayerLicenceResetResultDto>.Success(
                new PlayerLicenceResetResultDto(false, lastResetYear ?? currentCutoffYear, 0, 0, 0));
        }

        try
        {
            int floorball = await _floorballTeamRepository.DeactivateOpenPlayerLicencesAsync(cancellationToken);
            int football = await _footballTeamRepository.DeactivateOpenPlayerLicencesAsync(cancellationToken);
            int hockey = await _hockeyTeamRepository.DeactivateOpenPlayerLicencesAsync(cancellationToken);

            SiteSettingsEntity persisted = settings ?? SiteSettingsEntity.CreateDefault(Guid.NewGuid());
            if (settings is null)
            {
                await _siteSettingsRepository.AddAsync(persisted, cancellationToken);
            }

            persisted.MarkPlayerLicenceReset(today.Year);
            if (settings is not null)
            {
                await _siteSettingsRepository.UpdateAsync(persisted, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _siteSettingsProvider.Invalidate();

            _logger.LogInformation(
                "Player licence reset completed. Force: {Force}, Year: {Year}, Floorball: {Floorball}, Football: {Football}, Hockey: {Hockey}",
                request.Force,
                today.Year,
                floorball,
                football,
                hockey);

            return Result<PlayerLicenceResetResultDto>.Success(
                new PlayerLicenceResetResultDto(true, today.Year, floorball, football, hockey));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Player licence reset failed");
            return Result<PlayerLicenceResetResultDto>.Failure("Player licence reset failed.");
        }
        catch (ArgumentException ex)
        {
            return Result<PlayerLicenceResetResultDto>.Failure(ex.Message);
        }
    }

    private async Task EnsureSettingsInitializedAsync(
        SiteSettingsEntity? settings,
        int currentCutoffYear,
        CancellationToken cancellationToken)
    {
        if (settings is not null)
        {
            settings.MarkPlayerLicenceReset(currentCutoffYear);
            await _siteSettingsRepository.UpdateAsync(settings, cancellationToken);
        }
        else
        {
            SiteSettingsEntity created = SiteSettingsEntity.CreateDefault(Guid.NewGuid());
            created.MarkPlayerLicenceReset(currentCutoffYear);
            await _siteSettingsRepository.AddAsync(created, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _siteSettingsProvider.Invalidate();
    }
}
