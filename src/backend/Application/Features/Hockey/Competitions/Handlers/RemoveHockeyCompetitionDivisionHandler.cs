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
/// Handles removing a competition division.
/// </summary>
public class RemoveHockeyCompetitionDivisionHandler
    : IRequestHandler<RemoveHockeyCompetitionDivisionCommand, Result<HockeyCompetitionDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveHockeyCompetitionDivisionHandler> _logger;

    public RemoveHockeyCompetitionDivisionHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RemoveHockeyCompetitionDivisionHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyCompetitionDto>> Handle(
        RemoveHockeyCompetitionDivisionCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyCompetition? competition = await _competitionRepository.GetByIdAsync(request.CompetitionId);
            if (competition is null)
            {
                return Result<HockeyCompetitionDto>.NotFound("HockeyCompetition", request.CompetitionId);
            }

            competition.RemoveDivision(request.CompetitionDivisionId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Removed division {CompetitionDivisionId} from competition {CompetitionId}",
                request.CompetitionDivisionId,
                request.CompetitionId);

            return Result<HockeyCompetitionDto>.Success(HockeyCompetitionMapper.ToCompetitionDto(competition));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected RemoveDivision for {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed RemoveDivision for {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionDto>.Failure(
                "An error occurred while removing the division from the competition.",
                ex.Flatten());
        }
    }
}
