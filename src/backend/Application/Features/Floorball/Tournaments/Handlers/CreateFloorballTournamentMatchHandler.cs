using Application.Common;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Tournaments.Commands;
using Domain.Entities.Floorball;
using Domain.Entities.Floorball.Tournament;
using Domain.Enums.Floorball.Tournament;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Tournaments.Handlers;

public class CreateFloorballTournamentMatchHandler
    : IRequestHandler<CreateFloorballTournamentMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballRefereeRepository _refereeRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFloorballTournamentMatchHandler> _logger;

    public CreateFloorballTournamentMatchHandler(
        IFloorballTournamentRepository tournamentRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballMatchRepository matchRepository,
        IFloorballRefereeRepository refereeRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<CreateFloorballTournamentMatchHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _teamRepository = teamRepository;
        _matchRepository = matchRepository;
        _refereeRepository = refereeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(
        CreateFloorballTournamentMatchCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            FloorballTournament? tournament = await _tournamentRepository.GetByIdWithGroupsAsync(request.TournamentId);
            if (tournament == null)
                return Result<FloorballMatchDto>.NotFound("FloorballTournament", request.TournamentId);

            FloorballTeam? homeTeam = await _teamRepository.GetByIdAsync(request.HomeTeamId);
            if (homeTeam == null)
                return Result<FloorballMatchDto>.NotFound("FloorballTeam", request.HomeTeamId);

            FloorballTeam? awayTeam = await _teamRepository.GetByIdAsync(request.AwayTeamId);
            if (awayTeam == null)
                return Result<FloorballMatchDto>.NotFound("FloorballTeam", request.AwayTeamId);

            FloorballTournamentGroup? group = null;
            if (request.GroupId.HasValue)
            {
                group = tournament.Groups.FirstOrDefault(g => g.Id == request.GroupId.Value);
                if (group == null)
                    return Result<FloorballMatchDto>.NotFound("FloorballTournamentGroup", request.GroupId.Value);
            }

            FloorballTournamentRound? round = null;
            if (!string.IsNullOrWhiteSpace(request.TournamentRound))
            {
                if (!Enum.TryParse<FloorballTournamentRound>(request.TournamentRound, true, out FloorballTournamentRound parsed))
                    return Result<FloorballMatchDto>.Failure($"Invalid tournament round: '{request.TournamentRound}'");
                round = parsed;
            }

            DateTime scheduledDateTimeUtc = request.ScheduledDateTime.Kind switch
            {
                DateTimeKind.Utc => request.ScheduledDateTime,
                DateTimeKind.Local => request.ScheduledDateTime.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(request.ScheduledDateTime, DateTimeKind.Utc),
                _ => DateTime.SpecifyKind(request.ScheduledDateTime, DateTimeKind.Utc)
            };

            FloorballMatch match = new FloorballMatch(
                tournament,
                homeTeam,
                awayTeam,
                scheduledDateTimeUtc,
                request.Venue,
                group,
                round);

            FloorballReferee? referee = null;
            if (request.RefereeId.HasValue)
            {
                referee = await _refereeRepository.GetByIdAsync(request.RefereeId.Value);
                if (referee == null)
                    return Result<FloorballMatchDto>.NotFound("FloorballReferee", request.RefereeId.Value);
                match.AddOfficial(referee);
            }

            await _matchRepository.AddAsync(match);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballMatchDto dto = FloorballMatchMapper.ToDto(match);
            _logger.LogInformation(
                "Created tournament match {MatchId} for tournament {TournamentId}: {Home} vs {Away}",
                match.Id, tournament.Id, homeTeam.Name, awayTeam.Name);

            return Result<FloorballMatchDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating tournament match for tournament {TournamentId}", request.TournamentId);
            return Result<FloorballMatchDto>.Failure("An error occurred while creating the tournament match.");
        }
    }
}
