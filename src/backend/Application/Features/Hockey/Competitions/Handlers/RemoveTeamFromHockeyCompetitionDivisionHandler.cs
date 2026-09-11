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
/// Handles removing a competition team from a division.
/// </summary>
public class RemoveTeamFromHockeyCompetitionDivisionHandler
    : IRequestHandler<RemoveTeamFromHockeyCompetitionDivisionCommand, Result<HockeyCompetitionDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveTeamFromHockeyCompetitionDivisionHandler> _logger;

    public RemoveTeamFromHockeyCompetitionDivisionHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RemoveTeamFromHockeyCompetitionDivisionHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyCompetitionDto>> Handle(
        RemoveTeamFromHockeyCompetitionDivisionCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyCompetition? competition = await _competitionRepository.GetByIdAsync(request.CompetitionId);
            if (competition is null)
            {
                return Result<HockeyCompetitionDto>.NotFound("HockeyCompetition", request.CompetitionId);
            }

            competition.RemoveTeamFromDivision(request.CompetitionDivisionId, request.CompetitionTeamId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Removed competition team {CompetitionTeamId} from division {CompetitionDivisionId}",
                request.CompetitionTeamId,
                request.CompetitionDivisionId);

            return Result<HockeyCompetitionDto>.Success(HockeyCompetitionMapper.ToCompetitionDto(competition));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected RemoveTeamFromDivision for {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed RemoveTeamFromDivision for {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionDto>.Failure(
                "An error occurred while removing the team from the division.",
                ex.Flatten());
        }
    }
}
