using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Statistics.Queries;
using Application.Features.Floorball.Tournaments.Services;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Statistics.Handlers;

/// <summary>
/// Handler for retrieving standings for a single tournament group.
/// Calculates GP / W / D / L / GF / GA / GD / Pts from completed group-stage matches
/// using a 3-1-0 points convention to stay consistent with season standings.
/// </summary>
public class GetTournamentGroupStandingsHandler
    : IRequestHandler<GetTournamentGroupStandingsQuery, Result<List<FloorballTournamentGroupStandingDto>>>
{
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetTournamentGroupStandingsHandler> _logger;

    public GetTournamentGroupStandingsHandler(
        IFloorballTournamentRepository tournamentRepository,
        IFloorballMatchRepository matchRepository,
        IClubRepository clubRepository,
        ILogger<GetTournamentGroupStandingsHandler> logger)
    {
        _tournamentRepository = tournamentRepository;
        _matchRepository = matchRepository;
        _clubRepository = clubRepository;
        _logger = logger;
    }

    public async Task<Result<List<FloorballTournamentGroupStandingDto>>> Handle(
        GetTournamentGroupStandingsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Computing tournament group standings for Group: {GroupId}", request.GroupId);

            FloorballTournamentGroup? group =
                await _tournamentRepository.GetGroupByIdAsync(request.GroupId, cancellationToken);

            if (group == null)
            {
                _logger.LogWarning("Tournament group not found: {GroupId}", request.GroupId);
                return Result<List<FloorballTournamentGroupStandingDto>>.NotFound(
                    "Tournament group", request.GroupId.ToString());
            }

            // Resolve club logos so we can fall back when team logo is not set
            List<Guid> clubIds = group.Teams
                .Select(gt => gt.Team.ClubId)
                .Distinct()
                .ToList();

            Dictionary<Guid, Club> clubLookup = clubIds.Count == 0
                ? new Dictionary<Guid, Club>()
                : await _clubRepository.GetByIdsAsync(clubIds, cancellationToken);

            IEnumerable<FloorballMatch> completedMatches = await _matchRepository.GetByTournamentGroupAsync(
                request.GroupId,
                FloorballMatchStatus.Completed,
                cancellationToken);

            // Compute standings via the shared calculator so the playoff bracket generator and the
            // public group standings table apply the exact same tie-break ordering.
            List<TournamentStandingsCalculator.StandingsRow> rankedRows =
                TournamentStandingsCalculator.Compute(group, completedMatches);

            // Map each ranked row to a DTO (with team-logo enrichment).
            Dictionary<Guid, Uri?> teamLogos = group.Teams.ToDictionary(
                gt => gt.TeamId,
                gt => ResolveLogo(gt.Team, clubLookup));

            List<FloorballTournamentGroupStandingDto> standings = rankedRows
                .Select(r => new FloorballTournamentGroupStandingDto(
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

            return Result<List<FloorballTournamentGroupStandingDto>>.Success(standings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while computing standings for Group: {GroupId}", request.GroupId);
            return Result<List<FloorballTournamentGroupStandingDto>>.Failure(
                "An error occurred while retrieving tournament group standings.",
                ex.Flatten());
        }
    }

    private static Uri? ResolveLogo(FloorballTeam team, Dictionary<Guid, Club> clubLookup)
    {
        clubLookup.TryGetValue(team.ClubId, out Club? club);
        return team.GetEffectiveLogoUrl(club?.LogoUrl);
    }
}
