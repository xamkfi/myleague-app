using Application.Common;
using Application.Features.Hockey.Competitions.Commands;
using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Competitions.Mappings;
using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Competitions.Handlers;

public class AddTeamToHockeyCompetitionHandler
    : IRequestHandler<AddTeamToHockeyCompetitionCommand, Result<HockeyCompetitionTeamDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<AddTeamToHockeyCompetitionHandler> _logger;

    public AddTeamToHockeyCompetitionHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyTeamRepository teamRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<AddTeamToHockeyCompetitionHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyCompetitionTeamDto>> Handle(
        AddTeamToHockeyCompetitionCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team is null)
            {
                return Result<HockeyCompetitionTeamDto>.Failure("Hockey team not found.");
            }

            HockeyCompetition? competition = await _competitionRepository.GetByIdAsync(request.CompetitionId);
            if (competition is null)
            {
                return Result<HockeyCompetitionTeamDto>.Failure("Hockey competition not found.");
            }

            HockeyCompetitionTeam competitionTeam = competition.AddTeam(request.TeamId, request.Seed);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Added team {TeamId} to hockey competition {CompetitionId}",
                request.TeamId,
                request.CompetitionId);

            return Result<HockeyCompetitionTeamDto>.Success(HockeyCompetitionMapper.ToTeamDto(competitionTeam));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected add-team for competition {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionTeamDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add team to hockey competition {CompetitionId}", request.CompetitionId);
            return Result<HockeyCompetitionTeamDto>.Failure("An error occurred while adding the team to the competition.");
        }
    }
}
