using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Application.Features.Hockey.Matches.Queries;
using Domain.Entities.Hockey.Matches;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles listing matches for a competition.
/// </summary>
public class GetHockeyMatchesByCompetitionHandler
    : IRequestHandler<GetHockeyMatchesByCompetitionQuery, Result<IEnumerable<HockeyMatchDto>>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly ILogger<GetHockeyMatchesByCompetitionHandler> _logger;

    public GetHockeyMatchesByCompetitionHandler(
        IHockeyMatchRepository matchRepository,
        ILogger<GetHockeyMatchesByCompetitionHandler> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<HockeyMatchDto>>> Handle(
        GetHockeyMatchesByCompetitionQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<HockeyMatch> matches =
                await _matchRepository.GetByCompetitionIdAsync(request.CompetitionId);
            return Result<IEnumerable<HockeyMatchDto>>.Success(
                matches.Select(HockeyMatchMapper.ToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed GetHockeyMatchesByCompetition for {CompetitionId}",
                request.CompetitionId);
            return Result<IEnumerable<HockeyMatchDto>>.Failure(
                "An error occurred while retrieving hockey matches.",
                ex.Flatten());
        }
    }
}

/// <summary>
/// Handles listing matches for a career team.
/// </summary>
public class GetHockeyMatchesByTeamHandler
    : IRequestHandler<GetHockeyMatchesByTeamQuery, Result<IEnumerable<HockeyMatchDto>>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly ILogger<GetHockeyMatchesByTeamHandler> _logger;

    public GetHockeyMatchesByTeamHandler(
        IHockeyMatchRepository matchRepository,
        ILogger<GetHockeyMatchesByTeamHandler> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<HockeyMatchDto>>> Handle(
        GetHockeyMatchesByTeamQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<HockeyMatch> matches =
                await _matchRepository.GetByTeamIdAsync(request.TeamId);
            return Result<IEnumerable<HockeyMatchDto>>.Success(
                matches.Select(HockeyMatchMapper.ToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed GetHockeyMatchesByTeam for {TeamId}", request.TeamId);
            return Result<IEnumerable<HockeyMatchDto>>.Failure(
                "An error occurred while retrieving hockey matches.",
                ex.Flatten());
        }
    }
}
