using Application.Common;
using Application.Features.Football.Statistics.DTOs;
using Application.Features.Football.Statistics.Queries;
using Application.Features.Football.Tournaments.Services;
using Domain.Entities.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Enums.Football;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Statistics.Handlers;

/// <summary>
/// Handler for retrieving standings for a single tournament group.
/// Calculates GP / W / D / L / GF / GA / GD / Pts from completed group-stage matches
/// using the shared tournament standings calculator.
/// </summary>
public class GetTournamentGroupStandingsHandler
    : IRequestHandler<GetTournamentGroupStandingsQuery, Result<List<FootballTournamentGroupStandingDto>>>
{
    private readonly IFootballTournamentRepository _tournamentRepository;
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetTournamentGroupStandingsHandler> _logger;

    public GetTournamentGroupStandingsHandler(
        IFootballTournamentRepository tournamentRepository,
        IFootballMatchRepository matchRepository,
        IClubRepository clubRepository,
        ILogger<GetTournamentGroupStandingsHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _matchRepository = matchRepository;
        _clubRepository = clubRepository;
        _logger = logger;
    }

    public async Task<Result<List<FootballTournamentGroupStandingDto>>> Handle(
        GetTournamentGroupStandingsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Computing tournament group standings for Group: {GroupId}", request.GroupId);

            FootballTournamentGroup? group =
                await _tournamentRepository.GetGroupByIdAsync(request.GroupId, cancellationToken);

            if (group == null)
            {
                _logger.LogWarning("Tournament group not found: {GroupId}", request.GroupId);
                return Result<List<FootballTournamentGroupStandingDto>>.NotFound(
                    "Tournament group", request.GroupId.ToString());
            }

            List<Guid> clubIds = group.Teams
                .Select(gt => gt.Team.ClubId)
                .Distinct()
                .ToList();

            Dictionary<Guid, Club> clubLookup = clubIds.Count == 0
                ? new Dictionary<Guid, Club>()
                : await _clubRepository.GetByIdsAsync(clubIds, cancellationToken);

            IEnumerable<FootballMatch> completedMatches = await _matchRepository.GetByTournamentGroupAsync(
                request.GroupId,
                FootballMatchStatus.Completed,
                cancellationToken);

            List<TournamentStandingsCalculator.StandingsRow> rankedRows =
                TournamentStandingsCalculator.Compute(group, completedMatches);

            Dictionary<Guid, Uri?> teamLogos = group.Teams.ToDictionary(
                gt => gt.TeamId,
                gt => ResolveLogo(gt.Team, clubLookup));

            List<FootballTournamentGroupStandingDto> standings = rankedRows
                .Select(r => new FootballTournamentGroupStandingDto(
                    r.TeamId,
                    r.TeamName,
                    teamLogos.TryGetValue(r.TeamId, out Uri? logo) ? logo : null,
                    r.GamesPlayed,
                    r.Wins,
                    r.Draws,
                    r.Losses,
                    r.GoalsFor,
                    r.GoalsAgainst,
                    r.GoalDifference,
                    r.Points))
                .ToList();

            _logger.LogInformation(
                "Computed standings for Group: {GroupId} - {TeamCount} teams over {MatchCount} completed matches",
                request.GroupId, standings.Count, completedMatches.Count());

            return Result<List<FootballTournamentGroupStandingDto>>.Success(standings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while computing standings for Group: {GroupId}", request.GroupId);
            return Result<List<FootballTournamentGroupStandingDto>>.Failure(
                "An error occurred while retrieving tournament group standings.",
                ex.Flatten());
        }
    }

    private static Uri? ResolveLogo(FootballTeam team, Dictionary<Guid, Club> clubLookup)
    {
        clubLookup.TryGetValue(team.ClubId, out Club? club);
        return team.GetEffectiveLogoUrl(club?.LogoUrl);
    }
}
