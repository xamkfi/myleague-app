using Application.Common;
using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Competitions.Mappings;
using Application.Features.Hockey.Competitions.Queries;
using Domain.Entities.Hockey.Competitions;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Competitions.Handlers;

/// <summary>
/// Handles retrieving a hockey competition by id.
/// </summary>
public class GetHockeyCompetitionByIdHandler
    : IRequestHandler<GetHockeyCompetitionByIdQuery, Result<HockeyCompetitionDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly ILogger<GetHockeyCompetitionByIdHandler> _logger;

    public GetHockeyCompetitionByIdHandler(
        IHockeyCompetitionRepository competitionRepository,
        ILogger<GetHockeyCompetitionByIdHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _logger = logger;
    }

    public async Task<Result<HockeyCompetitionDto>> Handle(
        GetHockeyCompetitionByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyCompetition? competition = await _competitionRepository.GetByIdAsync(request.Id);
            if (competition is null)
            {
                return Result<HockeyCompetitionDto>.NotFound("HockeyCompetition", request.Id);
            }

            return Result<HockeyCompetitionDto>.Success(HockeyCompetitionMapper.ToCompetitionDto(competition));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hockey competition {CompetitionId}", request.Id);
            return Result<HockeyCompetitionDto>.Failure(
                "An error occurred while retrieving the hockey competition.",
                ex.Flatten());
        }
    }
}
