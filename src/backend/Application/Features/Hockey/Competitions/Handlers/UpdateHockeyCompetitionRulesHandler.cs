using Application.Common;
using Application.Features.Hockey.Competitions.Commands;
using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Competitions.Mappings;
using Domain.Entities.Hockey.Competitions;
using Domain.Repositories.Hockey;
using Domain.ValueObjects.Hockey.Rules;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Competitions.Handlers;

/// <summary>
/// Handles updating hockey competition rules.
/// </summary>
public class UpdateHockeyCompetitionRulesHandler
    : IRequestHandler<UpdateHockeyCompetitionRulesCommand, Result<HockeyCompetitionDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateHockeyCompetitionRulesHandler> _logger;

    public UpdateHockeyCompetitionRulesHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<UpdateHockeyCompetitionRulesHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyCompetitionDto>> Handle(
        UpdateHockeyCompetitionRulesCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyCompetition? competition = await _competitionRepository.GetByIdAsync(request.CompetitionId);
            if (competition is null)
            {
                return Result<HockeyCompetitionDto>.NotFound("HockeyCompetition", request.CompetitionId);
            }

            HockeyCompetitionRules rules = new(
                request.Name,
                request.RuleBookVersion,
                request.RuleBookSource,
                HockeyMatchRules.Default(),
                HockeyStandingRules.Default(),
                HockeyRosterRules.Default(),
                HockeyVideoReviewRules.Disabled(),
                HockeyContactRules.Default());

            competition.UpdateCompetitionRules(rules);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated competition rules for {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionDto>.Success(HockeyCompetitionMapper.ToCompetitionDto(competition));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected UpdateCompetitionRules for {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid UpdateCompetitionRules for {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed UpdateCompetitionRules for {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionDto>.Failure(
                "An error occurred while updating competition rules.",
                ex.Flatten());
        }
    }
}
