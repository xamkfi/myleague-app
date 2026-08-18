using Application.Common;
using Application.Features.Football.Tournaments.DTOs;
using Application.Features.Football.Tournaments.Queries;
using Domain.Entities.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Entities.Football.Statistics;
using Domain.Enums.Football;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Tournaments.Handlers;

/// <summary>
/// Read-side handler that returns the playoff bracket for a tournament, grouped by round, with
/// denormalized team logo/name and forward references so the frontend can render the diagram in a
/// single round-trip.
/// </summary>
public class GetTournamentPlayoffBracketHandler
    : IRequestHandler<GetTournamentPlayoffBracketQuery, Result<FootballPlayoffBracketDto>>
{
    private readonly IFootballTournamentRepository _tournamentRepository;
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetTournamentPlayoffBracketHandler> _logger;

    public GetTournamentPlayoffBracketHandler(
        IFootballTournamentRepository tournamentRepository,
        IFootballMatchRepository matchRepository,
        IFootballTeamRepository teamRepository,
        IClubRepository clubRepository,
        ILogger<GetTournamentPlayoffBracketHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _logger = logger;
    }

    public async Task<Result<FootballPlayoffBracketDto>> Handle(
        GetTournamentPlayoffBracketQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            FootballTournament? tournament = await _tournamentRepository.GetByIdAsync(request.CompetitionId, cancellationToken);
            if (tournament == null)
            {
                _logger.LogWarning("Tournament not found while fetching playoff bracket: {TournamentId}", request.CompetitionId);
                return Result<FootballPlayoffBracketDto>.NotFound("FootballTournament", request.CompetitionId);
            }

            IEnumerable<FootballMatch> all = await _matchRepository.GetByCompetitionIdAsync(request.CompetitionId);
            List<FootballMatch> playoffMatches = all.Where(m => m.PlayoffRound != null).ToList();

            // Build a quick lookup of "is feeder match for X.Y completed?" so the frontend can show TBD.
            Dictionary<(Guid, FootballPlayoffSlot), bool> feederResolved =
                new Dictionary<(Guid, FootballPlayoffSlot), bool>();
            foreach (FootballMatch m in playoffMatches)
            {
                if (m.NextMatchId.HasValue && m.NextMatchSlot.HasValue)
                {
                    feederResolved[(m.NextMatchId.Value, m.NextMatchSlot.Value)] =
                        m.Status == FootballMatchStatus.Completed;
                }
            }

            // Resolve team logos with the same fallback the rest of the app uses (team logo > club logo).
            // Skip slots that haven't been assigned a team yet (null) — they render as TBD.
            HashSet<Guid> teamIds = playoffMatches
                .SelectMany(m => new Guid?[] { m.HomeTeamId, m.AwayTeamId })
                .Where(id => id.HasValue && id.Value != Guid.Empty)
                .Select(id => id!.Value)
                .ToHashSet();
            if (tournament.ChampionTeamId.HasValue)
            {
                teamIds.Add(tournament.ChampionTeamId.Value);
            }
            Dictionary<Guid, FootballTeam> teamLookup = new Dictionary<Guid, FootballTeam>();
            foreach (Guid id in teamIds)
            {
                FootballTeam? team = await _teamRepository.GetByIdAsync(id);
                if (team != null)
                {
                    teamLookup[id] = team;
                }
            }
            List<Guid> clubIds = teamLookup.Values.Select(t => t.ClubId).Distinct().ToList();
            Dictionary<Guid, Club> clubLookup = clubIds.Count == 0
                ? new Dictionary<Guid, Club>()
                : await _clubRepository.GetByIdsAsync(clubIds, cancellationToken);

            FootballPlayoffTeamDto? ToTeamDto(Guid? teamId)
            {
                if (!teamId.HasValue || teamId.Value == Guid.Empty
                    || !teamLookup.TryGetValue(teamId.Value, out FootballTeam? team))
                {
                    return null;
                }
                Uri? logo = team.GetEffectiveLogoUrl(
                    clubLookup.TryGetValue(team.ClubId, out Club? club) ? club.LogoUrl : null);
                return new FootballPlayoffTeamDto(team.Id, team.Name, logo?.ToString());
            }

            // Group matches by round, ordered by match order within each round, then by round display order.
            FootballPlayoffRound[] roundOrder = new[]
            {
                FootballPlayoffRound.QuarterFinal,
                FootballPlayoffRound.SemiFinal,
                FootballPlayoffRound.ThirdPlaceMatch,
                FootballPlayoffRound.Final
            };

            List<FootballPlayoffRoundDto> roundDtos = new List<FootballPlayoffRoundDto>();
            foreach (FootballPlayoffRound round in roundOrder)
            {
                List<FootballMatch> matchesInRound = playoffMatches
                    .Where(m => m.PlayoffRound == round)
                    .OrderBy(m => m.PlayoffMatchOrder ?? 0)
                    .ToList();
                if (matchesInRound.Count == 0)
                {
                    continue;
                }
                List<FootballPlayoffMatchDto> matchDtos = new List<FootballPlayoffMatchDto>();
                foreach (FootballMatch m in matchesInRound)
                {
                    bool homeFeederResolved = !feederResolved.ContainsKey((m.Id, FootballPlayoffSlot.Home))
                        || feederResolved[(m.Id, FootballPlayoffSlot.Home)];
                    bool awayFeederResolved = !feederResolved.ContainsKey((m.Id, FootballPlayoffSlot.Away))
                        || feederResolved[(m.Id, FootballPlayoffSlot.Away)];

                    matchDtos.Add(new FootballPlayoffMatchDto(
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
                roundDtos.Add(new FootballPlayoffRoundDto(round.ToString(), matchDtos));
            }

            FootballPlayoffTeamDto? championDto = tournament.ChampionTeamId.HasValue
                ? ToTeamDto(tournament.ChampionTeamId.Value)
                : null;

            FootballPlayoffBracketDto dto = new FootballPlayoffBracketDto(
                TournamentId: tournament.Id,
                TournamentStatus: tournament.TournamentStatus.ToString(),
                HasThirdPlaceMatch: tournament.TournamentRules.HasThirdPlaceMatch,
                Champion: championDto,
                Rounds: roundDtos);

            return Result<FootballPlayoffBracketDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching playoff bracket for tournament: {TournamentId}", request.CompetitionId);
            return Result<FootballPlayoffBracketDto>.Failure(
                "An error occurred while fetching the tournament playoff bracket.",
                ex.Flatten());
        }
    }
}
