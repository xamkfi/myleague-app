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
/// Handles creation of a group within a hockey tournament.
/// </summary>
public class CreateHockeyTournamentGroupHandler
    : IRequestHandler<CreateHockeyTournamentGroupCommand, Result<HockeyTournamentDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<CreateHockeyTournamentGroupHandler> _logger;

    public CreateHockeyTournamentGroupHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<CreateHockeyTournamentGroupHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTournamentDto>> Handle(
        CreateHockeyTournamentGroupCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyTournament? tournament = await _competitionRepository.GetTournamentByIdAsync(request.TournamentId);
            if (tournament is null)
            {
                return Result<HockeyTournamentDto>.NotFound("HockeyTournament", request.TournamentId);
            }

            HockeyTournamentGroup group = tournament.AddGroup(request.Name);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created hockey tournament group {GroupId} ({Name}) on tournament {TournamentId}",
                group.Id,
                group.Name,
                tournament.Id);

            return Result<HockeyTournamentDto>.Success(HockeyCompetitionMapper.ToTournamentDto(tournament));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Domain rejected create-group for tournament {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid create-group request for tournament {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Failure(ex.Message, ex.Flatten());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create group for hockey tournament {TournamentId}", request.TournamentId);
            return Result<HockeyTournamentDto>.Failure("An error occurred while creating the tournament group.", ex.Flatten());
        }
    }
}
