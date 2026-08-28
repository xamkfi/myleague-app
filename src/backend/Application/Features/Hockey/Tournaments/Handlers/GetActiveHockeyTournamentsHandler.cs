using Application.Common;
using Application.Features.Hockey.Competitions.Mappings;
using Application.Features.Hockey.Tournaments.DTOs;
using Application.Features.Hockey.Tournaments.Queries;
using Domain.Entities.Hockey.Competitions;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Tournaments.Handlers;

/// <summary>
/// Handles retrieving active hockey tournaments.
/// </summary>
public class GetActiveHockeyTournamentsHandler
    : IRequestHandler<GetActiveHockeyTournamentsQuery, Result<IEnumerable<HockeyTournamentDto>>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly ILogger<GetActiveHockeyTournamentsHandler> _logger;

    public GetActiveHockeyTournamentsHandler(
        IHockeyCompetitionRepository competitionRepository,
        ILogger<GetActiveHockeyTournamentsHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<HockeyTournamentDto>>> Handle(
        GetActiveHockeyTournamentsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<HockeyTournament> tournaments = await _competitionRepository.GetAllTournamentsAsync();
            List<HockeyTournamentDto> active = tournaments
                .Where(t => t.IsActive)
                .Where(t => request.TeamCategory is null || t.TeamCategory == request.TeamCategory)
                .Select(HockeyCompetitionMapper.ToTournamentDto)
                .ToList();

            return Result<IEnumerable<HockeyTournamentDto>>.Success(active);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get active hockey tournaments");
            return Result<IEnumerable<HockeyTournamentDto>>.Failure(
                "An error occurred while retrieving active hockey tournaments.",
                ex.Flatten());
        }
    }
}
