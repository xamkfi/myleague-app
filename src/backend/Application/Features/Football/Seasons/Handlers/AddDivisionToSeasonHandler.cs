using Application.Common;
using Application.Features.Football.Seasons.Commands;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Seasons.Handlers;

public class AddDivisionToSeasonHandler : IRequestHandler<AddDivisionToSeasonCommand, Result>
{
    private readonly IFootballCompetitionRepository _seasonRepository;
    private readonly IFootballCompetitionDivisionRepository _seasonDivisionRepository;
    private readonly ILogger<AddDivisionToSeasonHandler> _logger;

    public AddDivisionToSeasonHandler(
        IFootballCompetitionRepository seasonRepository,
        IFootballCompetitionDivisionRepository seasonDivisionRepository,
        ILogger<AddDivisionToSeasonHandler> logger)
    {
        _seasonRepository = seasonRepository;
        _seasonDivisionRepository = seasonDivisionRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(AddDivisionToSeasonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!await _seasonRepository.ExistsAsync(request.CompetitionId))
            {
                return Result.NotFound("FootballSeason", request.CompetitionId);
            }

            _logger.LogInformation("Adding division {DivisionId} to season {SeasonId}", request.DivisionId, request.CompetitionId);
            await _seasonDivisionRepository.AddCompetitionDivisionAsync(request.CompetitionId, request.DivisionId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding division {DivisionId} to season {SeasonId}", request.DivisionId, request.CompetitionId);
            return Result.Failure("Failed to add division to season.");
        }
    }
}
