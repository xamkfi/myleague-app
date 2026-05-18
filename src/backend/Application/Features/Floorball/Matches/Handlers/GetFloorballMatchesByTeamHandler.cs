// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Teams.Mappings;
using Application.Features.Floorball.Players.Mappings;
using Application.Features.Floorball.Referees.Mappings;
using Application.Features.Floorball.TeamManagers.Mappings;
using Application.Features.Floorball.Statistics.Mappings;
using Application.Features.Floorball.Matches.Queries;
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Matches.Handlers
{
    public class GetFloorballMatchesByTeamHandler : BasePagedQueryHandler<GetFloorballMatchesByTeamQuery, FloorballMatchDto>,
        IRequestHandler<GetFloorballMatchesByTeamQuery, Result<PagedResult<FloorballMatchDto>>>
    {
        private readonly IFloorballMatchRepository _floorballMatchRepository;

        /// <summary>
        /// Initializes a new instance of the GetAllFloorballMatchesHandler class
        /// </summary>
        /// <param name="floorballMatchRepository"></param>
        /// <param name="paginationService"></param>
        public GetFloorballMatchesByTeamHandler(
            IFloorballMatchRepository floorballMatchRepository,
            IPaginationService paginationService,
            ILogger<GetFloorballMatchesByTeamHandler> logger) : base (paginationService, logger)
        {
            _floorballMatchRepository = floorballMatchRepository;
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

                IEnumerable<FloorballMatchDto> matchDtos = FloorballMatchMapper.ToDtos(pagedMatches.Items);

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
