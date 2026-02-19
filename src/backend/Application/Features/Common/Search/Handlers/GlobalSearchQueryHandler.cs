// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Common.Divisions.Queries;
using Application.Features.Common.Search.Queries;
using Application.Features.Common.MatchTimer.Queries;
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Entities.Common;

namespace Application.Features.Common.Search.Handlers
{
    public class GlobalSearchQueryHandler : IRequestHandler<GlobalSearchQuery, Result<GlobalSearchResultDto>>
    {
        private readonly IFloorballTeamRepository _floorballTeamRepository;
        private readonly IClubRepository _clubRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IFloorballPlayerRepository _floorballPlayerRepository;
        private readonly ILogger<GlobalSearchQueryHandler> _logger;

        public GlobalSearchQueryHandler(
            IFloorballTeamRepository floorballTeamRepository,
            IClubRepository clubRepository,
            IPersonRepository personRepository,
            IFloorballPlayerRepository floorballPlayerRepository,
            ILogger<GlobalSearchQueryHandler> logger)
        {
            _floorballTeamRepository = floorballTeamRepository;
            _clubRepository = clubRepository;
            _personRepository = personRepository;
            _floorballPlayerRepository = floorballPlayerRepository;
            _logger = logger;
        }

        public async Task<Result<GlobalSearchResultDto>> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GlobalSearchQuery with SearchTerm: {SearchTerm}", request.SearchTerm);

            if (string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                return Result<GlobalSearchResultDto>.Success(new GlobalSearchResultDto(
                    Array.Empty<GlobalSearchResultPersonDto>(),
                    Array.Empty<GlobalSearchResultTeamDto>(),
                    Array.Empty<string>()));
            }

            cancellationToken.ThrowIfCancellationRequested();

            string searchTerm = request.SearchTerm;
            const int maxResultsPerEntityType = 5;

            IEnumerable<Club> clubs = await _clubRepository.SearchByNameAsync(searchTerm, maxResultsPerEntityType, cancellationToken);
            PagedResult<Person> personsResult = await _personRepository.SearchByNameAsync(searchTerm, page: 1, pageSize: maxResultsPerEntityType, cancellationToken);
            IEnumerable<Person> persons = personsResult.Items;
            IEnumerable<FloorballTeam> teams = await _floorballTeamRepository.SearchByNameAsync(searchTerm, maxResultsPerEntityType, cancellationToken);

            // Enrich person data
            IEnumerable<Guid> personIds = persons.Select(p => p.Id);
            Dictionary<Guid, FloorballPlayer> playerMap = await _floorballPlayerRepository.GetByPersonIdsAsync(personIds, cancellationToken);
            IEnumerable<Guid> floorballPlayerIds = playerMap.Values.Select(pl => pl.Id);
            Dictionary<Guid, FloorballTeam> playerTeamMap = await _floorballTeamRepository.GetTeamsByPlayerIdsAsync(floorballPlayerIds, cancellationToken);

            // Get club IDs from both player teams and direct team search results
            IEnumerable<Guid> playerTeamClubIds = playerTeamMap.Values.Select(t => t.ClubId).Distinct();
            IEnumerable<Guid> teamClubIds = teams.Select(t => t.ClubId).Distinct();
            IEnumerable<Guid> allClubIds = playerTeamClubIds.Union(teamClubIds);
            Dictionary<Guid, Club> clubMap = await _clubRepository.GetByIdsAsync(allClubIds, cancellationToken);

            GlobalSearchResultPersonDto[] personResults = persons.Select(p =>
            {
                if (!playerMap.TryGetValue(p.Id, out FloorballPlayer? fp))
                {
                    // This person is not a floorball player, return basic person data
                    return new GlobalSearchResultPersonDto(p.Id, p.FirstName, p.LastName);
                }
                
                // This person is a floorball player, enrich with team data (may be null if not on a team)
                playerTeamMap.TryGetValue(fp.Id, out FloorballTeam? team);
                Club? club = team != null && clubMap.TryGetValue(team.ClubId, out Club? c) ? c : null;
                return new GlobalSearchResultPersonDto(fp.Id, p.FirstName, p.LastName, team?.Id, team?.Name, club?.Id, club?.Name);
            }).ToArray();

            GlobalSearchResultTeamDto[] teamResults = teams
                .Select(t => 
                {
                    Club? club = clubMap.TryGetValue(t.ClubId, out Club? c) ? c : null;
                    return new GlobalSearchResultTeamDto(t.Id, t.Name, t.ClubId, club?.Name);
                })
                .ToArray();

            string[] clubNames = clubs.Select(c => c.Name).ToArray();

            GlobalSearchResultDto combinedResult = new GlobalSearchResultDto(personResults, teamResults, clubNames);

            return Result<GlobalSearchResultDto>.Success(combinedResult);
        }
    }
}
