using Application.Common;
using Application.Features.Hockey.Competitions.Mappings;
using Application.Features.Hockey.Tournaments.Commands;
using Application.Features.Hockey.Tournaments.DTOs;
using Domain.Entities.Hockey.Competitions;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Tournaments.Handlers;

/// <summary>
/// Handles assigning teams to a hockey playoff series.
/// </summary>
public class AssignHockeyPlayoffSeriesTeamsHandler
    : IRequestHandler<AssignHockeyPlayoffSeriesTeamsCommand, Result<HockeyTournamentDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<AssignHockeyPlayoffSeriesTeamsHandler> _logger;

    public AssignHockeyPlayoffSeriesTeamsHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<AssignHockeyPlayoffSeriesTeamsHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTournamentDto>> Handle(
        AssignHockeyPlayoffSeriesTeamsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyTournament? tournament = await _competitionRepository.GetTournamentByIdAsync(request.TournamentId);
            if (tournament is null)
            {
                return Result<HockeyTournamentDto>.NotFound("HockeyTournament", request.TournamentId);
            }

            tournament.AssignPlayoffSeriesTeams(
                request.SeriesId,
                request.HomeCompetitionTeamId,
                request.AwayCompetitionTeamId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Assigned teams to playoff series {SeriesId} on tournament {TournamentId}",
                request.SeriesId,
                request.TournamentId);

            return Result<HockeyTournamentDto>.Success(HockeyCompetitionMapper.ToTournamentDto(tournament));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected assign playoff teams for {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid assign playoff teams for {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed assign playoff teams for {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Failure("An error occurred while assigning playoff series teams.", ex.Flatten());
        }
    }
}
