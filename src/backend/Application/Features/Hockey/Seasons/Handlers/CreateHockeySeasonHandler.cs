using Application.Common;
using Application.Features.Hockey.Competitions.Mappings;
using Application.Features.Hockey.Seasons.Commands;
using Application.Features.Hockey.Seasons.DTOs;
using Domain.Entities.Hockey.Competitions;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Seasons.Handlers;

/// <summary>
/// Handles creation of a new hockey season.
/// </summary>
public class CreateHockeySeasonHandler : IRequestHandler<CreateHockeySeasonCommand, Result<HockeySeasonDto>>
{
    private readonly IHockeyCompetitionRepository _competitionRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<CreateHockeySeasonHandler> _logger;

    public CreateHockeySeasonHandler(
        IHockeyCompetitionRepository competitionRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<CreateHockeySeasonHandler> logger)
    {
        _competitionRepository = competitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeySeasonDto>> Handle(CreateHockeySeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            HockeySeason season = new(
                request.Name,
                request.StartDate,
                request.EndDate,
                request.SeasonCode,
                teamCategory: request.TeamCategory);
            await _competitionRepository.AddAsync(season);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created hockey season {SeasonId} ({Name})", season.Id, season.Name);
            return Result<HockeySeasonDto>.Success(HockeyCompetitionMapper.ToSeasonDto(season));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create hockey season {Name}", request.Name);
            return Result<HockeySeasonDto>.Failure("An error occurred while creating the hockey season.", ex.Flatten());
        }
    }
}
