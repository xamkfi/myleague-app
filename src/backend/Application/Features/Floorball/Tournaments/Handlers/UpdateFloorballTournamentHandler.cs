using Application.Features.Floorball.Tournaments.Commands;
using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Tournaments.Mappings;
using Application.Common;
using Domain.Entities.Floorball.Tournament;
using Domain.Enums.Floorball.Tournament;
using Domain.Repositories.Floorball;
using Domain.ValueObjects.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Handlers;

public class UpdateFloorballTournamentHandler : IRequestHandler<UpdateFloorballTournamentCommand, Result<FloorballTournamentDto>>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFloorballTournamentHandler> _logger;

    public UpdateFloorballTournamentHandler(
        IFloorballTournamentRepository tournamentRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<UpdateFloorballTournamentHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballTournamentDto>> Handle(UpdateFloorballTournamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballTournament? tournament = await _tournamentRepository.GetByIdAsync(request.Id);
            if (tournament is null)
            {
                _logger.LogWarning("Attempt to update non-existent floorball tournament with ID: {TournamentId}", request.Id);
                return Result<FloorballTournamentDto>.NotFound("FloorballTournament", request.Id);
            }

            if (!Enum.TryParse<FloorballTournamentPlayoffFormat>(request.PlayoffFormat, true, out FloorballTournamentPlayoffFormat format))
            {
                return Result<FloorballTournamentDto>.Failure($"Invalid playoff format: '{request.PlayoffFormat}'.");
            }

            DateTime startDateUtc = request.StartDate.Kind switch
            {
                DateTimeKind.Utc => request.StartDate,
                DateTimeKind.Local => request.StartDate.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc),
                _ => DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc)
            };

            DateTime endDateUtc = request.EndDate.Kind switch
            {
                DateTimeKind.Utc => request.EndDate,
                DateTimeKind.Local => request.EndDate.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc),
                _ => DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc)
            };

            FloorballMatchRules matchRules = new(
                request.NumberOfPeriods,
                request.PeriodDurationMinutes,
                request.AllowOvertime,
                request.OvertimeDurationMinutes,
                request.AllowShootout);

            tournament.UpdateDetails(request.Name, startDateUtc, endDateUtc, request.Location, request.DescriptionHtml);
            tournament.UpdateMatchRules(matchRules);
            tournament.UpdatePlayoffSettings(format, request.GroupStageAdvancingCount);

            _logger.LogInformation("Updating floorball tournament with ID: {TournamentId}", request.Id);
            await _tournamentRepository.UpdateAsync(tournament);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballTournamentDto dto = FloorballTournamentMapper.ToDto(tournament);
            _logger.LogInformation("Successfully updated floorball tournament with ID: {TournamentId}", tournament.Id);

            return Result<FloorballTournamentDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating floorball tournament: {TournamentId}", request.Id);
            return Result<FloorballTournamentDto>.Failure("An error occurred while updating the floorball tournament.");
        }
    }
}
