// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.Search.Queries;
using Domain.Common;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Football.Teams;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Football;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Search.Handlers
{
    public class GlobalSearchQueryHandler : IRequestHandler<GlobalSearchQuery, Result<GlobalSearchResultDto>>
    {
        private readonly IFloorballTeamRepository _floorballTeamRepository;
        private readonly IFootballTeamRepository _footballTeamRepository;
        private readonly IHockeyTeamRepository _hockeyTeamRepository;
        private readonly IClubRepository _clubRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IFloorballPlayerRepository _floorballPlayerRepository;
        private readonly IFootballPlayerRepository _footballPlayerRepository;
        private readonly ILogger<GlobalSearchQueryHandler> _logger;

        public GlobalSearchQueryHandler(
            IFloorballTeamRepository floorballTeamRepository,
            IFootballTeamRepository footballTeamRepository,
            IHockeyTeamRepository hockeyTeamRepository,
            IClubRepository clubRepository,
            IPersonRepository personRepository,
            IFloorballPlayerRepository floorballPlayerRepository,
            IFootballPlayerRepository footballPlayerRepository,
            ILogger<GlobalSearchQueryHandler> logger)
        {
            _floorballTeamRepository = floorballTeamRepository;
            _footballTeamRepository = footballTeamRepository;
            _hockeyTeamRepository = hockeyTeamRepository;
            _clubRepository = clubRepository;
            _personRepository = personRepository;
            _floorballPlayerRepository = floorballPlayerRepository;
            _footballPlayerRepository = footballPlayerRepository;
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

            IEnumerable<FloorballTeam> floorballTeams = await _floorballTeamRepository.SearchByNameAsync(searchTerm, maxResultsPerEntityType, cancellationToken);
            IEnumerable<FootballTeam> footballTeams = await _footballTeamRepository.SearchByNameAsync(searchTerm, maxResultsPerEntityType, cancellationToken);
            PagedResult<HockeyTeam> hockeyTeamsPage = await _hockeyTeamRepository.GetPagedAsync(
                page: 1,
                pageSize: maxResultsPerEntityType,
                searchTerm: searchTerm,
                cancellationToken: cancellationToken);

            IEnumerable<Guid> personIds = persons.Select(p => p.Id);
            Dictionary<Guid, FloorballPlayer> floorballPlayerMap = await _floorballPlayerRepository.GetByPersonIdsAsync(personIds, cancellationToken);
            Dictionary<Guid, FootballPlayer> footballPlayerMap = await _footballPlayerRepository.GetByPersonIdsAsync(personIds, cancellationToken);

            IEnumerable<Guid> floorballPlayerIds = floorballPlayerMap.Values.Select(player => player.Id);
            IEnumerable<Guid> footballPlayerIds = footballPlayerMap.Values.Select(player => player.Id);
            Dictionary<Guid, FloorballTeam> floorballPlayerTeamMap = await _floorballTeamRepository.GetTeamsByPlayerIdsAsync(floorballPlayerIds, cancellationToken);
            Dictionary<Guid, FootballTeam> footballPlayerTeamMap = await _footballTeamRepository.GetTeamsByPlayerIdsAsync(footballPlayerIds, cancellationToken);

            IEnumerable<Guid> allClubIds = floorballPlayerTeamMap.Values.Select(team => team.ClubId)
                .Union(footballPlayerTeamMap.Values.Select(team => team.ClubId))
                .Union(floorballTeams.Select(team => team.ClubId))
                .Union(footballTeams.Select(team => team.ClubId))
                .Union(hockeyTeamsPage.Items.Select(team => team.ClubId))
                .Distinct();
            Dictionary<Guid, Club> clubMap = await _clubRepository.GetByIdsAsync(allClubIds, cancellationToken);

            GlobalSearchResultPersonDto[] personResults = persons.Select(person =>
            {
                if (floorballPlayerMap.TryGetValue(person.Id, out FloorballPlayer? floorballPlayer))
                {
                    floorballPlayerTeamMap.TryGetValue(floorballPlayer.Id, out FloorballTeam? team);
                    Club? club = team != null && clubMap.TryGetValue(team.ClubId, out Club? mappedClub) ? mappedClub : null;
                    return new GlobalSearchResultPersonDto(
                        floorballPlayer.Id,
                        person.FirstName,
                        person.LastName,
                        team?.Id,
                        team?.Name,
                        club?.Id,
                        club?.Name,
                        "floorball");
                }

                if (footballPlayerMap.TryGetValue(person.Id, out FootballPlayer? footballPlayer))
                {
                    footballPlayerTeamMap.TryGetValue(footballPlayer.Id, out FootballTeam? team);
                    Club? club = team != null && clubMap.TryGetValue(team.ClubId, out Club? mappedClub) ? mappedClub : null;
                    return new GlobalSearchResultPersonDto(
                        footballPlayer.Id,
                        person.FirstName,
                        person.LastName,
                        team?.Id,
                        team?.Name,
                        club?.Id,
                        club?.Name,
                        "football");
                }

                return new GlobalSearchResultPersonDto(person.Id, person.FirstName, person.LastName);
            }).ToArray();

            List<GlobalSearchResultTeamDto> teamResults = new();
            teamResults.AddRange(floorballTeams.Select(team =>
            {
                Club? club = clubMap.TryGetValue(team.ClubId, out Club? mappedClub) ? mappedClub : null;
                return new GlobalSearchResultTeamDto(team.Id, team.Name, team.ClubId, club?.Name, "floorball");
            }));
            teamResults.AddRange(footballTeams.Select(team =>
            {
                Club? club = clubMap.TryGetValue(team.ClubId, out Club? mappedClub) ? mappedClub : null;
                return new GlobalSearchResultTeamDto(team.Id, team.Name, team.ClubId, club?.Name, "football");
            }));
            teamResults.AddRange(hockeyTeamsPage.Items.Select(team =>
            {
                Club? club = clubMap.TryGetValue(team.ClubId, out Club? mappedClub) ? mappedClub : null;
                return new GlobalSearchResultTeamDto(team.Id, team.Name, team.ClubId, club?.Name, "hockey");
            }));

            string[] clubNames = clubs.Select(club => club.Name).ToArray();
            GlobalSearchResultDto combinedResult = new GlobalSearchResultDto(
                personResults,
                teamResults.Take(maxResultsPerEntityType * 3).ToArray(),
                clubNames);

            return Result<GlobalSearchResultDto>.Success(combinedResult);
        }
    }
}
