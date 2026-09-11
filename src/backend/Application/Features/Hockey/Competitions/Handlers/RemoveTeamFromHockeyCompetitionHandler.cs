using Application.Common;
using Application.Features.Hockey.Competitions.Commands;
using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Competitions.Mappings;
using Domain.Entities.Hockey.Competitions;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Competitions.Handlers;

/// <summary>
/// Handles removing a team from a hockey competition.
/// </summary>
public class RemoveTeamFromHockeyCompetitionHandler
    : IRequestHandler<RemoveTeamFromHockeyCompetitionCommand, Result<HockeyCompetitionDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveTeamFromHockeyCompetitionHandler> _logger;

    public RemoveTeamFromHockeyCompetitionHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RemoveTeamFromHockeyCompetitionHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyCompetitionDto>> Handle(
        RemoveTeamFromHockeyCompetitionCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyCompetition? competition = await _competitionRepository.GetByIdAsync(request.CompetitionId);
            if (competition is null)
            {
                return Result<HockeyCompetitionDto>.NotFound("HockeyCompetition", request.CompetitionId);
            }

            competition.RemoveTeam(request.TeamId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Removed team {TeamId} from hockey competition {CompetitionId}",
                request.TeamId,
                request.CompetitionId);

            return Result<HockeyCompetitionDto>.Success(HockeyCompetitionMapper.ToCompetitionDto(competition));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected RemoveTeam for competition {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid RemoveTeam for competition {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed RemoveTeam for competition {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionDto>.Failure(
                "An error occurred while removing the team from the competition.",
                ex.Flatten());
        }
    }
}
