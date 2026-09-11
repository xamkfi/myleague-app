using Application.Common;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Application.Features.Football.Matches.Queries;
using Domain.Entities.Common;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class GetFootballMatchByIdHandler : IRequestHandler<GetFootballMatchByIdQuery, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetFootballMatchByIdHandler> _logger;

    public GetFootballMatchByIdHandler(
        IFootballMatchRepository matchRepository,
        IFootballPlayerRepository playerRepository,
        IPersonRepository personRepository,
        IClubRepository clubRepository,
        ILogger<GetFootballMatchByIdHandler> logger)
    {
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _personRepository = personRepository;
        _clubRepository = clubRepository;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(GetFootballMatchByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.Id);
            if (match == null)
            {
                return Result<FootballMatchDto>.NotFound("FootballMatch", request.Id);
            }

            IEnumerable<Guid> playerIds = match.GoalEvents
                .SelectMany(g => new[] { g.ScoringPlayerId, g.AssistingPlayerId })
                .Concat(match.CardEvents.Select(c => (Guid?)c.PlayerId))
                .Concat(match.SubstitutionEvents.SelectMany(s => new Guid?[] { s.PlayerOffId, s.PlayerOnId }))
                .Concat(match.Lineup.Select(p => (Guid?)p.PlayerId))
                .OfType<Guid>()
                .Distinct()
                .ToList();

            Dictionary<Guid, Person> playerPersonLookup = new();
            if (playerIds.Any())
            {
                List<FootballPlayer> players = new();
                foreach (Guid playerId in playerIds)
                {
                    FootballPlayer? player = await _playerRepository.GetByIdAsync(playerId);
                    if (player != null)
                    {
                        players.Add(player);
                    }
                }

                List<Guid> personIds = players.Select(p => p.PersonId).Distinct().ToList();
                if (personIds.Any())
                {
                    IEnumerable<Person> persons = await _personRepository.GetByIdsAsync(personIds);
                    Dictionary<Guid, Person> personLookup = persons.ToDictionary(p => p.Id, p => p);
                    foreach (FootballPlayer player in players)
                    {
                        if (personLookup.TryGetValue(player.PersonId, out Person? person))
                        {
                            playerPersonLookup[player.Id] = person;
                        }
                    }
                }
            }

            List<Guid> clubIds = new[] { match.HomeTeam?.ClubId, match.AwayTeam?.ClubId }
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            Dictionary<Guid, Club> clubLookup = clubIds.Count == 0
                ? new Dictionary<Guid, Club>()
                : await _clubRepository.GetByIdsAsync(clubIds, cancellationToken);

            Club? homeClub = null;
            Club? awayClub = null;
            if (match.HomeTeam != null)
            {
                clubLookup.TryGetValue(match.HomeTeam.ClubId, out homeClub);
            }

            if (match.AwayTeam != null)
            {
                clubLookup.TryGetValue(match.AwayTeam.ClubId, out awayClub);
            }

            FootballMatchDto matchDto = FootballMatchMapper.ToDto(match, playerPersonLookup, homeClub, awayClub);
            return Result<FootballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving football match: {MatchId}", request.Id);
            return Result<FootballMatchDto>.Failure("An error occurred while retrieving the football match.");
        }
    }
}
