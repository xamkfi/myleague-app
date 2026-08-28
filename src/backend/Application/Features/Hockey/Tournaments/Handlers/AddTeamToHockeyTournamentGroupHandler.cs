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
/// Handles adding a competition team to a hockey tournament group.
/// </summary>
public class AddTeamToHockeyTournamentGroupHandler
    : IRequestHandler<AddTeamToHockeyTournamentGroupCommand, Result<HockeyTournamentDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<AddTeamToHockeyTournamentGroupHandler> _logger;

    public AddTeamToHockeyTournamentGroupHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<AddTeamToHockeyTournamentGroupHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTournamentDto>> Handle(
        AddTeamToHockeyTournamentGroupCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyTournament? tournament = await _competitionRepository.GetTournamentByIdAsync(request.TournamentId);
            if (tournament is null)
            {
                return Result<HockeyTournamentDto>.NotFound("HockeyTournament", request.TournamentId);
            }

            tournament.AddTeamToGroup(request.GroupId, request.CompetitionTeamId, request.Seed);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Added competition team {CompetitionTeamId} to group {GroupId} on tournament {TournamentId}",
                request.CompetitionTeamId,
                request.GroupId,
                request.TournamentId);

            return Result<HockeyTournamentDto>.Success(HockeyCompetitionMapper.ToTournamentDto(tournament));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                ex,
                "Domain rejected add-team-to-group for tournament {TournamentId}, group {GroupId}",
                request.TournamentId,
                request.GroupId);
            return Result<HockeyTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "Invalid add-team-to-group request for tournament {TournamentId}, group {GroupId}",
                request.TournamentId,
                request.GroupId);
            return Result<HockeyTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to add competition team to group {GroupId} on tournament {TournamentId}",
                request.GroupId,
                request.TournamentId);
            return Result<HockeyTournamentDto>.Failure("An error occurred while adding the team to the tournament group.", ex.Flatten());
        }
    }
}
