using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Statistics.Queries;
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

            // Initialize standings rows for every team that belongs to the group, even with 0 GP
            Dictionary<Guid, GroupStandingAccumulator> rows = group.Teams.ToDictionary(
                gt => gt.TeamId,
                gt => new GroupStandingAccumulator(gt.Team, ResolveLogo(gt.Team, clubLookup)));

            IEnumerable<FloorballMatch> completedMatches = await _matchRepository.GetByTournamentGroupAsync(
                request.GroupId,
                FloorballMatchStatus.Completed,
                cancellationToken);

            foreach (FloorballMatch match in completedMatches)
            {
                ApplyMatch(rows, match);
            }

            List<FloorballTournamentGroupStandingDto> standings = rows.Values
                .OrderByDescending(r => r.Points)
                .ThenByDescending(r => r.GoalsFor - r.GoalsAgainst)
                .ThenByDescending(r => r.GoalsFor)
                .ThenBy(r => r.TeamName, StringComparer.OrdinalIgnoreCase)
                .Select(r => new FloorballTournamentGroupStandingDto(
                    r.TeamId,
                    r.TeamName,
                    r.TeamLogo,
                    r.GamesPlayed,
                    r.Wins,
                    r.Draws,
                    r.Losses,
                    r.GoalsFor,
                    r.GoalsAgainst,
                    r.GoalsFor - r.GoalsAgainst,
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

    private static void ApplyMatch(Dictionary<Guid, GroupStandingAccumulator> rows, FloorballMatch match)
    {
        bool homeKnown = rows.TryGetValue(match.HomeTeamId, out GroupStandingAccumulator? home);
        bool awayKnown = rows.TryGetValue(match.AwayTeamId, out GroupStandingAccumulator? away);

        // Skip matches where neither team is part of the group (defensive guard).
        if (!homeKnown && !awayKnown)
        {
            return;
        }

        int homeScore = match.HomeScore;
        int awayScore = match.AwayScore;

        if (homeKnown && home != null)
        {
            home.AddResult(scoredFor: homeScore, scoredAgainst: awayScore);
        }

        if (awayKnown && away != null)
        {
            away.AddResult(scoredFor: awayScore, scoredAgainst: homeScore);
        }
    }

    private static Uri? ResolveLogo(FloorballTeam team, Dictionary<Guid, Club> clubLookup)
    {
        clubLookup.TryGetValue(team.ClubId, out Club? club);
        return team.GetEffectiveLogoUrl(club?.LogoUrl);
    }

    /// <summary>
    /// Mutable accumulator used while folding match results into per-team standings rows.
    /// </summary>
    private sealed class GroupStandingAccumulator
    {
        public Guid TeamId { get; }
        public string TeamName { get; }
        public Uri? TeamLogo { get; }
        public int GamesPlayed { get; private set; }
        public int Wins { get; private set; }
        public int Draws { get; private set; }
        public int Losses { get; private set; }
        public int GoalsFor { get; private set; }
        public int GoalsAgainst { get; private set; }
        public int Points { get; private set; }

        public GroupStandingAccumulator(FloorballTeam team, Uri? logo)
        {
            TeamId = team.Id;
            TeamName = team.Name;
            TeamLogo = logo;
        }

        public void AddResult(int scoredFor, int scoredAgainst)
        {
            GamesPlayed++;
            GoalsFor += scoredFor;
            GoalsAgainst += scoredAgainst;

            if (scoredFor > scoredAgainst)
            {
                Wins++;
                Points += 3;
            }
            else if (scoredFor < scoredAgainst)
            {
                Losses++;
            }
            else
            {
                Draws++;
                Points += 1;
            }
        }
    }
}
