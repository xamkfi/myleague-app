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
/// Handles creation of a new hockey tournament.
/// </summary>
public class CreateHockeyTournamentHandler : IRequestHandler<CreateHockeyTournamentCommand, Result<HockeyTournamentDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<CreateHockeyTournamentHandler> _logger;

    public CreateHockeyTournamentHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<CreateHockeyTournamentHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyTournamentDto>> Handle(CreateHockeyTournamentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            HockeyTournament tournament = new(
                request.Name,
                DateTimeUtc.Normalize(request.StartDate),
                DateTimeUtc.Normalize(request.EndDate),
                request.Venue,
                request.ContentHtml,
                teamCategory: request.TeamCategory);

            await _competitionRepository.AddAsync(tournament);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created hockey tournament {TournamentId} ({Name})", tournament.Id, tournament.Name);
            return Result<HockeyTournamentDto>.Success(HockeyCompetitionMapper.ToTournamentDto(tournament));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create hockey tournament {Name}", request.Name);
            return Result<HockeyTournamentDto>.Failure("An error occurred while creating the hockey tournament.", ex.Flatten());
        }
    }
}
