using Application.Common;
using Application.Features.Hockey.Competitions.Mappings;
using Application.Features.Hockey.Tournaments.DTOs;
using Application.Features.Hockey.Tournaments.Queries;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Tournaments.Handlers;

/// <summary>
/// Handles retrieving all hockey tournaments.
/// </summary>
public class GetAllHockeyTournamentsHandler : IRequestHandler<GetAllHockeyTournamentsQuery, Result<IEnumerable<HockeyTournamentDto>>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly ILogger<GetAllHockeyTournamentsHandler> _logger;

    public GetAllHockeyTournamentsHandler(
        IHockeyCompetitionRepository competitionRepository,
        ILogger<GetAllHockeyTournamentsHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<HockeyTournamentDto>>> Handle(
        GetAllHockeyTournamentsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<Domain.Entities.Hockey.Competitions.HockeyTournament> tournaments =
                await _competitionRepository.GetAllTournamentsAsync();
            IEnumerable<HockeyTournamentDto> dtos = tournaments.Select(HockeyCompetitionMapper.ToTournamentDto);
            return Result<IEnumerable<HockeyTournamentDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list hockey tournaments");
            return Result<IEnumerable<HockeyTournamentDto>>.Failure(
                "An error occurred while retrieving hockey tournaments.",
                ex.Flatten());
        }
    }
}
