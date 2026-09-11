using Application.Common;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class CreateFootballMatchHandler : IRequestHandler<CreateFootballMatchCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballCompetitionRepository _seasonRepository;
    private readonly IFootballRefereeRepository _refereeRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFootballMatchHandler> _logger;

    public CreateFootballMatchHandler(
        IFootballMatchRepository matchRepository,
        IFootballTeamRepository teamRepository,
        IFootballCompetitionRepository seasonRepository,
        IFootballRefereeRepository refereeRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<CreateFootballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _seasonRepository = seasonRepository;
        _refereeRepository = refereeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(CreateFootballMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!request.CompetitionId.HasValue)
            {
                return Result<FootballMatchDto>.Failure("Competition is required");
            }

            FootballCompetition? competition = await _seasonRepository.GetByIdAsync(request.CompetitionId);
            if (competition == null)
            {
                _logger.LogWarning("Attempt to create match for non-existent competition with ID: {CompetitionId}", request.CompetitionId);
                return Result<FootballMatchDto>.NotFound("FootballCompetition", request.CompetitionId ?? Guid.Empty);
            }

            FootballTeam? homeTeam = null;
            if (request.HomeTeamId.HasValue)
            {
                homeTeam = await _teamRepository.GetByIdAsync(request.HomeTeamId.Value);
                if (homeTeam == null)
                {
                    _logger.LogWarning("Attempt to create match with non-existent home team ID: {TeamId}", request.HomeTeamId);
                    return Result<FootballMatchDto>.NotFound("FootballTeam", request.HomeTeamId.Value);
                }
            }

            FootballTeam? awayTeam = null;
            if (request.AwayTeamId.HasValue)
            {
                awayTeam = await _teamRepository.GetByIdAsync(request.AwayTeamId.Value);
                if (awayTeam == null)
                {
                    _logger.LogWarning("Attempt to create match with non-existent away team ID: {TeamId}", request.AwayTeamId);
                    return Result<FootballMatchDto>.NotFound("FootballTeam", request.AwayTeamId.Value);
                }
            }

            FootballReferee? referee = null;
            if (request.RefereeId.HasValue)
            {
                referee = await _refereeRepository.GetByIdAsync(request.RefereeId.Value);
                if (referee == null)
                {
                    _logger.LogWarning("Attempt to create match with non-existent referee ID: {RefereeId}", request.RefereeId);
                    return Result<FootballMatchDto>.NotFound("FootballReferee", request.RefereeId.Value);
                }
            }

            FootballMatch match = FootballMatchMapper.ToEntity(request, competition, homeTeam, awayTeam, referee);

            _logger.LogInformation(
                "Creating new football match between teams: {HomeTeamId} vs {AwayTeamId}",
                request.HomeTeamId,
                request.AwayTeamId);
            await _matchRepository.AddAsync(match);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FootballMatchDto matchDto = FootballMatchMapper.ToDto(match);
            _logger.LogInformation("Successfully created football match with ID: {MatchId}", match.Id);

            return Result<FootballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while creating football match between teams: {HomeTeamId} vs {AwayTeamId}",
                request.HomeTeamId,
                request.AwayTeamId);
            return Result<FootballMatchDto>.Failure("An error occurred while creating the football match.");
        }
    }
}
