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
/// Handles placing a competition team into a division.
/// </summary>
public class AddTeamToHockeyCompetitionDivisionHandler
    : IRequestHandler<AddTeamToHockeyCompetitionDivisionCommand, Result<HockeyCompetitionDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<AddTeamToHockeyCompetitionDivisionHandler> _logger;

    public AddTeamToHockeyCompetitionDivisionHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<AddTeamToHockeyCompetitionDivisionHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyCompetitionDto>> Handle(
        AddTeamToHockeyCompetitionDivisionCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyCompetition? competition = await _competitionRepository.GetByIdAsync(request.CompetitionId);
            if (competition is null)
            {
                return Result<HockeyCompetitionDto>.NotFound("HockeyCompetition", request.CompetitionId);
            }

            competition.AddTeamToDivision(
                request.CompetitionDivisionId,
                request.CompetitionTeamId,
                request.Seed);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Added competition team {CompetitionTeamId} to division {CompetitionDivisionId}",
                request.CompetitionTeamId,
                request.CompetitionDivisionId);

            return Result<HockeyCompetitionDto>.Success(HockeyCompetitionMapper.ToCompetitionDto(competition));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected AddTeamToDivision for {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid AddTeamToDivision for {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed AddTeamToDivision for {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionDto>.Failure(
                "An error occurred while adding the team to the division.",
                ex.Flatten());
        }
    }
}
