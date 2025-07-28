// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Common;
using Application.Queries.Common;
using Application.Services.Common;
using Domain.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Entities.Common;

namespace Application.Handlers.Common
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
            IEnumerable<Person> persons = await _personRepository.SearchByNameAsync(searchTerm, maxResultsPerEntityType, cancellationToken);
            IEnumerable<FloorballTeam> teams = await _floorballTeamRepository.SearchByNameAsync(searchTerm, maxResultsPerEntityType, cancellationToken);

            // Enrich person data
            IEnumerable<Guid> personIds = persons.Select(p => p.Id);
            Dictionary<Guid, FloorballPlayer> playerMap = await _floorballPlayerRepository.GetByPersonIdsAsync(personIds, cancellationToken);
            IEnumerable<Guid> floorballPlayerIds = playerMap.Values.Select(pl => pl.Id);
            Dictionary<Guid, FloorballTeam> playerTeamMap = await _floorballTeamRepository.GetTeamsByPlayerIdsAsync(floorballPlayerIds, cancellationToken);

            IEnumerable<Guid> clubIds = playerTeamMap.Values.Select(t => t.ClubId).Distinct();
            Dictionary<Guid, Club> clubMap = await _clubRepository.GetByIdsAsync(clubIds, cancellationToken);

            GlobalSearchResultPersonDto[] personResults = persons.Select(p =>
            {
                if (!playerMap.TryGetValue(p.Id, out FloorballPlayer? fp))
                {
                    return new GlobalSearchResultPersonDto(p.Id, p.FirstName, p.LastName, null, null, null, null);
                }
                playerTeamMap.TryGetValue(fp.Id, out FloorballTeam? team);
                Club? club = team != null && clubMap.TryGetValue(team.ClubId, out Club? c) ? c : null;
                return new GlobalSearchResultPersonDto(p.Id, p.FirstName, p.LastName, team?.Id, team?.Name, club?.Id, club?.Name);
            }).ToArray();

            GlobalSearchResultTeamDto[] teamResults = teams
                .Select(t => new GlobalSearchResultTeamDto(t.Id, t.Name, t.ClubId, null))
                .ToArray();

            string[] clubNames = clubs.Select(c => c.Name).ToArray();

            GlobalSearchResultDto combinedResult = new GlobalSearchResultDto(personResults, teamResults, clubNames);

            return Result<GlobalSearchResultDto>.Success(combinedResult);
        }
    }
}
