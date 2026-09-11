// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Features.Floorball.Matches.Queries;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Mappings;
using Application.Common;
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Floorball.Matches.Handlers
{
    public class GetFloorballMatchesByTeamHandler : BasePagedQueryHandler<GetFloorballMatchesByTeamQuery, FloorballMatchDto>,
        IRequestHandler<GetFloorballMatchesByTeamQuery, Result<PagedResult<FloorballMatchDto>>>
    {
        private readonly IFloorballMatchRepository _floorballMatchRepository;
        private readonly IClubRepository _clubRepository;

        public GetFloorballMatchesByTeamHandler(
            IFloorballMatchRepository floorballMatchRepository,
            IClubRepository clubRepository,
            IPaginationService paginationService,
            ILogger<GetFloorballMatchesByTeamHandler> logger) : base (paginationService, logger)
        {
            _floorballMatchRepository = floorballMatchRepository;
            _clubRepository = clubRepository;
        }

        public async Task<Result<PagedResult<FloorballMatchDto>>> Handle(GetFloorballMatchesByTeamQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting all floorball matches for team: {team}", request.TeamId);
                Result<PaginationValidationResult> validationResult = ValidatePaginationParameters(
                    request.Page, request.PageSize, GetFloorballMatchesByTeamQuery.ResourceKey);

                if (validationResult.IsFailure)
                {
                    return Result<PagedResult<FloorballMatchDto>>.Failure(validationResult.Error!);
                }

                int actualPageSize = validationResult.Data!.ActualPageSize;

                cancellationToken.ThrowIfCancellationRequested();

                PagedResult<FloorballMatch> pagedMatches = await _floorballMatchRepository.GetPagedAsync(
                    page: request.Page,
                    pageSize: actualPageSize,
                    competitionId: null,
                    teamId: request.TeamId,
                    startDate: request.StartDate,
                    endDate: request.EndDate,
                    status: null,
                    cancellationToken: cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                IEnumerable<FloorballMatch> matchItems = pagedMatches.Items ?? Enumerable.Empty<FloorballMatch>();
                List<Guid> clubIds = FloorballMatchMapper.CollectClubIds(matchItems);
                Dictionary<Guid, Club> clubLookup = clubIds.Count == 0
                    ? new Dictionary<Guid, Club>()
                    : await _clubRepository.GetByIdsAsync(clubIds, cancellationToken);
                IEnumerable<FloorballMatchDto> matchDtos = FloorballMatchMapper.ToDtos(matchItems, clubLookup);

                PagedResult<FloorballMatchDto> pagedResult = CreatePagedResult(
                    matchDtos,
                    pagedMatches.TotalCount,
                    pagedMatches.Page,
                    actualPageSize);

                return Result<PagedResult<FloorballMatchDto>>.Success(pagedResult);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Floorball matches for a team was cancelled - Page: {Page}, PageSize: {PageSize}",
                    request.Page, request.PageSize);
                throw;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving floorball matches for a team");
                return Result<PagedResult<FloorballMatchDto>>.Failure("Error occurred while retrieving floorball matches for a team");
            }
        }
    }
}
