using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Tournaments.Queries;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Tournaments.Handlers;

/// <summary>
/// Read-side handler that returns the playoff bracket for a tournament, grouped by round, with
/// denormalized team logo/name and forward references so the frontend can render the diagram in a
/// single round-trip.
/// </summary>
public class GetTournamentPlayoffBracketHandler
    : IRequestHandler<GetTournamentPlayoffBracketQuery, Result<FloorballPlayoffBracketDto>>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetTournamentPlayoffBracketHandler> _logger;

    public GetTournamentPlayoffBracketHandler(
        IFloorballTournamentRepository tournamentRepository,
        IFloorballMatchRepository matchRepository,
        IFloorballTeamRepository teamRepository,
        IClubRepository clubRepository,
        ILogger<GetTournamentPlayoffBracketHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _logger = logger;
    }

    public async Task<Result<FloorballPlayoffBracketDto>> Handle(
        GetTournamentPlayoffBracketQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            FloorballTournament? tournament = await _tournamentRepository.GetByIdAsync(request.CompetitionId, cancellationToken);
            if (tournament == null)
            {
                _logger.LogWarning("Tournament not found while fetching playoff bracket: {TournamentId}", request.CompetitionId);
                return Result<FloorballPlayoffBracketDto>.NotFound("FloorballTournament", request.CompetitionId);
            }

            IEnumerable<FloorballMatch> all = await _matchRepository.GetByCompetitionIdAsync(request.CompetitionId);
            List<FloorballMatch> playoffMatches = all.Where(m => m.PlayoffRound != null).ToList();

            // Build a quick lookup of "is feeder match for X.Y completed?" so the frontend can show TBD.
            Dictionary<(Guid, FloorballPlayoffSlot), bool> feederResolved =
                new Dictionary<(Guid, FloorballPlayoffSlot), bool>();
            foreach (FloorballMatch m in playoffMatches)
            {
                if (m.NextMatchId.HasValue && m.NextMatchSlot.HasValue)
                {
                    feederResolved[(m.NextMatchId.Value, m.NextMatchSlot.Value)] =
                        m.Status == FloorballMatchStatus.Completed;
                }
            }

            // Resolve team logos with the same fallback the rest of the app uses (team logo > club logo).
            HashSet<Guid> teamIds = playoffMatches
                .SelectMany(m => new[] { m.HomeTeamId, m.AwayTeamId })
                .Where(id => id != Guid.Empty)
                .ToHashSet();
            if (tournament.ChampionTeamId.HasValue)
            {
                teamIds.Add(tournament.ChampionTeamId.Value);
            }
            Dictionary<Guid, FloorballTeam> teamLookup = new Dictionary<Guid, FloorballTeam>();
            foreach (Guid id in teamIds)
            {
                FloorballTeam? team = await _teamRepository.GetByIdAsync(id);
                if (team != null)
                {
                    teamLookup[id] = team;
                }
            }
            List<Guid> clubIds = teamLookup.Values.Select(t => t.ClubId).Distinct().ToList();
            Dictionary<Guid, Club> clubLookup = clubIds.Count == 0
                ? new Dictionary<Guid, Club>()
                : await _clubRepository.GetByIdsAsync(clubIds, cancellationToken);

            FloorballPlayoffTeamDto? ToTeamDto(Guid teamId)
            {
                if (!teamLookup.TryGetValue(teamId, out FloorballTeam? team))
                {
                    return null;
                }
                Uri? logo = team.GetEffectiveLogoUrl(
                    clubLookup.TryGetValue(team.ClubId, out Club? club) ? club.LogoUrl : null);
                return new FloorballPlayoffTeamDto(team.Id, team.Name, logo?.ToString());
            }

            // Group matches by round, ordered by match order within each round, then by round display order.
            FloorballPlayoffRound[] roundOrder = new[]
            {
                FloorballPlayoffRound.QuarterFinal,
                FloorballPlayoffRound.SemiFinal,
                FloorballPlayoffRound.ThirdPlaceMatch,
                FloorballPlayoffRound.Final
            };

            List<FloorballPlayoffRoundDto> roundDtos = new List<FloorballPlayoffRoundDto>();
            foreach (FloorballPlayoffRound round in roundOrder)
            {
                List<FloorballMatch> matchesInRound = playoffMatches
                    .Where(m => m.PlayoffRound == round)
                    .OrderBy(m => m.PlayoffMatchOrder ?? 0)
                    .ToList();
                if (matchesInRound.Count == 0)
                {
                    continue;
                }
                List<FloorballPlayoffMatchDto> matchDtos = new List<FloorballPlayoffMatchDto>();
                foreach (FloorballMatch m in matchesInRound)
                {
                    bool homeFeederResolved = !feederResolved.ContainsKey((m.Id, FloorballPlayoffSlot.Home))
                        || feederResolved[(m.Id, FloorballPlayoffSlot.Home)];
                    bool awayFeederResolved = !feederResolved.ContainsKey((m.Id, FloorballPlayoffSlot.Away))
                        || feederResolved[(m.Id, FloorballPlayoffSlot.Away)];

                    matchDtos.Add(new FloorballPlayoffMatchDto(
                        MatchId: m.Id,
                        Order: m.PlayoffMatchOrder ?? 0,
                        Status: m.Status.ToString(),
                        ScheduledDateTime: m.ScheduledDateTime.ToUniversalTime(),
                        Venue: m.Venue,
                        HomeScore: m.HomeScore,
                        AwayScore: m.AwayScore,
                        HomeTeam: ToTeamDto(m.HomeTeamId),
                        AwayTeam: ToTeamDto(m.AwayTeamId),
                        IsHomeFeederResolved: homeFeederResolved,
                        IsAwayFeederResolved: awayFeederResolved,
                        NextMatchId: m.NextMatchId,
                        NextMatchSlot: m.NextMatchSlot?.ToString()));
                }
                roundDtos.Add(new FloorballPlayoffRoundDto(round.ToString(), matchDtos));
            }

            FloorballPlayoffTeamDto? championDto = tournament.ChampionTeamId.HasValue
                ? ToTeamDto(tournament.ChampionTeamId.Value)
                : null;

            FloorballPlayoffBracketDto dto = new FloorballPlayoffBracketDto(
                TournamentId: tournament.Id,
                TournamentStatus: tournament.TournamentStatus.ToString(),
                HasThirdPlaceMatch: tournament.TournamentRules.HasThirdPlaceMatch,
                Champion: championDto,
                Rounds: roundDtos);

            return Result<FloorballPlayoffBracketDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching playoff bracket for tournament: {TournamentId}", request.CompetitionId);
            return Result<FloorballPlayoffBracketDto>.Failure(
                "An error occurred while fetching the tournament playoff bracket.",
                ex.Flatten());
        }
    }
}
