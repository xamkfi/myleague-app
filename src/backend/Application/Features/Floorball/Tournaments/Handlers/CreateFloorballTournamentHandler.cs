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

public class CreateFloorballTournamentHandler : IRequestHandler<CreateFloorballTournamentCommand, Result<FloorballTournamentDto>>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFloorballTournamentHandler> _logger;

    public CreateFloorballTournamentHandler(
        IFloorballTournamentRepository tournamentRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<CreateFloorballTournamentHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballTournamentDto>> Handle(CreateFloorballTournamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<FloorballTournamentPlayoffFormat>(request.PlayoffFormat, true, out FloorballTournamentPlayoffFormat format))
            {
                return Result<FloorballTournamentDto>.Failure($"Invalid playoff format: '{request.PlayoffFormat}'.");
            }

            FloorballMatchRules matchRules = new(
                request.NumberOfPeriods,
                request.PeriodDurationMinutes,
                request.AllowOvertime,
                request.OvertimeDurationMinutes,
                request.AllowShootout);

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

            FloorballTournament tournament = new(
                request.Name,
                startDateUtc,
                endDateUtc,
                request.Location,
                request.DescriptionHtml,
                matchRules,
                format,
                request.GroupStageAdvancingCount);

            _logger.LogInformation("Creating new floorball tournament: {Name}", request.Name);
            await _tournamentRepository.AddAsync(tournament);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballTournamentDto dto = FloorballTournamentMapper.ToDto(tournament);
            _logger.LogInformation("Successfully created floorball tournament with ID: {TournamentId}", tournament.Id);

            return Result<FloorballTournamentDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating floorball tournament: {Name}", request.Name);
            return Result<FloorballTournamentDto>.Failure("An error occurred while creating the floorball tournament.");
        }
    }
}
