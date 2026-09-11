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
/// Handles retrieving a hockey tournament by id.
/// </summary>
public class GetHockeyTournamentByIdHandler : IRequestHandler<GetHockeyTournamentByIdQuery, Result<HockeyTournamentDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly ILogger<GetHockeyTournamentByIdHandler> _logger;

    public GetHockeyTournamentByIdHandler(
        IHockeyCompetitionRepository competitionRepository,
        ILogger<GetHockeyTournamentByIdHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _logger = logger;
    }

    public async Task<Result<HockeyTournamentDto>> Handle(GetHockeyTournamentByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            HockeyTournament? tournament = await _competitionRepository.GetTournamentByIdAsync(request.Id);
            if (tournament is null)
            {
                return Result<HockeyTournamentDto>.NotFound("HockeyTournament", request.Id);
            }

            return Result<HockeyTournamentDto>.Success(HockeyCompetitionMapper.ToTournamentDto(tournament));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hockey tournament {TournamentId}", request.Id);
            return Result<HockeyTournamentDto>.Failure("An error occurred while retrieving the hockey tournament.", ex.Flatten());
        }
    }
}
