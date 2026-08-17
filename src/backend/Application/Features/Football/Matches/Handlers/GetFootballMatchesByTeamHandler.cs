using Application.Common;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Application.Features.Football.Matches.Queries;
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Football.Matches;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

public class GetFootballMatchesByTeamHandler : BasePagedQueryHandler<GetFootballMatchesByTeamQuery, FootballMatchDto>,
    IRequestHandler<GetFootballMatchesByTeamQuery, Result<PagedResult<FootballMatchDto>>>
{
    private readonly IFootballMatchRepository _footballMatchRepository;

    public GetFootballMatchesByTeamHandler(
        IFootballMatchRepository footballMatchRepository,
        IPaginationService paginationService,
        ILogger<GetFootballMatchesByTeamHandler> logger) : base(paginationService, logger)
    {
        _footballMatchRepository = footballMatchRepository;
    }

    public async Task<Result<PagedResult<FootballMatchDto>>> Handle(
        GetFootballMatchesByTeamQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            Result<PaginationValidationResult> validationResult = ValidatePaginationParameters(
                request.Page,
                request.PageSize,
                GetFootballMatchesByTeamQuery.ResourceKey);

            if (validationResult.IsFailure)
            {
                return Result<PagedResult<FootballMatchDto>>.Failure(validationResult.Error!);
            }

            int actualPageSize = validationResult.Data!.ActualPageSize;

            PagedResult<FootballMatch> pagedMatches = await _footballMatchRepository.GetPagedAsync(
                page: request.Page,
                pageSize: actualPageSize,
                competitionId: null,
                teamId: request.TeamId,
                startDate: request.StartDate,
                endDate: request.EndDate,
                status: null,
                cancellationToken: cancellationToken);

            IEnumerable<FootballMatchDto> matchDtos = FootballMatchMapper.ToDtos(pagedMatches.Items);
            PagedResult<FootballMatchDto> pagedResult = CreatePagedResult(
                matchDtos,
                pagedMatches.TotalCount,
                pagedMatches.Page,
                actualPageSize);

            return Result<PagedResult<FootballMatchDto>>.Success(pagedResult);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving football matches for a team");
            return Result<PagedResult<FootballMatchDto>>.Failure("Error occurred while retrieving football matches for a team");
        }
    }
}
