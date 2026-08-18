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
/// Handles getting effective hockey competition rules.
/// </summary>
public class GetEffectiveHockeyCompetitionRulesHandler
    : IRequestHandler<GetEffectiveHockeyCompetitionRulesQuery, Result<HockeyCompetitionRulesDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly ILogger<GetEffectiveHockeyCompetitionRulesHandler> _logger;

    public GetEffectiveHockeyCompetitionRulesHandler(
        IHockeyCompetitionRepository competitionRepository,
        ILogger<GetEffectiveHockeyCompetitionRulesHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _logger = logger;
    }

    public async Task<Result<HockeyCompetitionRulesDto>> Handle(
        GetEffectiveHockeyCompetitionRulesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyCompetition? competition = await _competitionRepository.GetByIdAsync(request.CompetitionId);
            if (competition is null)
            {
                return Result<HockeyCompetitionRulesDto>.NotFound("HockeyCompetition", request.CompetitionId);
            }

            return Result<HockeyCompetitionRulesDto>.Success(
                HockeyCompetitionMapper.ToCompetitionRulesDto(competition.GetEffectiveRules()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed GetEffectiveHockeyCompetitionRules for {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionRulesDto>.Failure(
                "An error occurred while retrieving effective competition rules.",
                ex.Flatten());
        }
    }
}
