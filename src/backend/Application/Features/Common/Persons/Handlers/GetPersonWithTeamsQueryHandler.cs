using Application.Common;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Common.Persons.Queries;
using Domain.Entities.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.Persons.Handlers;

/// <summary>
/// Handler for GetPersonWithTeamsQuery
/// </summary>
public class GetPersonWithTeamsQueryHandler : IRequestHandler<GetPersonWithTeamsQuery, Result<PersonWithTeamsDto>>
{
    private readonly IPersonRepository _personRepository;
    private readonly IFloorballPlayerRepository _floorballPlayerRepository;
    private readonly IFloorballTeamRepository _floorballTeamRepository;
    private readonly ILogger<GetPersonWithTeamsQueryHandler> _logger;

    public GetPersonWithTeamsQueryHandler(
        IPersonRepository personRepository,
        IFloorballPlayerRepository floorballPlayerRepository,
        IFloorballTeamRepository floorballTeamRepository,
        ILogger<GetPersonWithTeamsQueryHandler> logger)
    {
        _personRepository = personRepository;
        _floorballPlayerRepository = floorballPlayerRepository;
        _floorballTeamRepository = floorballTeamRepository;
        _logger = logger;
    }

    public async Task<Result<PersonWithTeamsDto>> Handle(GetPersonWithTeamsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting person with teams for PersonId: {PersonId}", request.PersonId);

            // Get the person
            Person? person = await _personRepository.GetByIdAsync(request.PersonId);

            if (person == null)
            {
                return Result<PersonWithTeamsDto>.Failure($"Person with ID {request.PersonId} not found");
            }

            // Get the floorball player profile
            FloorballPlayer? floorballPlayer = await _floorballPlayerRepository.GetByPersonIdAsync(request.PersonId);

            if (floorballPlayer == null)
            {
                // Person exists but has no floorball player profile
                return Result<PersonWithTeamsDto>.Success(new PersonWithTeamsDto(
                    GetPersonIdAsInt(person.Id),
                    person.FullName,
                    CalculateAge(person.BirthDate) ?? 0,
                    new List<PersonTeamDto>(),
                    0
                ));
            }

            // Get all teams where this player is in the roster
            IEnumerable<FloorballTeam> teamsEnumerable = await _floorballTeamRepository.GetByPlayerIdAsync(floorballPlayer.Id);
            List<FloorballTeam> teams = teamsEnumerable.ToList();

            int totalMatchesPlayed = 0;
            List<PersonTeamDto> personTeams = new List<PersonTeamDto>();

            foreach (FloorballTeam team in teams)
            {
                FloorballTeamPlayer? playerInTeam = team.Roster.FirstOrDefault(r => r.PlayerId == floorballPlayer.Id);
                if (playerInTeam == null) continue;

                totalMatchesPlayed += playerInTeam.GamesPlayed;

                bool isGoalkeeper = playerInTeam.Position == FloorballPosition.Goalkeeper;

                PersonPlayerDto playerDto = new PersonPlayerDto(
                    GetPersonIdAsInt(person.Id),
                    playerInTeam.JerseyNumber,
                    playerInTeam.GamesPlayed,
                    isGoalkeeper ? null : playerInTeam.Goals,
                    isGoalkeeper ? null : playerInTeam.Assists
                );

                List<PersonPlayerDto> goalkeepers = isGoalkeeper ? new List<PersonPlayerDto> { playerDto } : new List<PersonPlayerDto>();
                List<PersonPlayerDto> fieldPlayers = isGoalkeeper ? new List<PersonPlayerDto>() : new List<PersonPlayerDto> { playerDto };

                PersonTeamDto teamDto = new PersonTeamDto(
                    GetTeamIdAsInt(team.Id),
                    team.Name,
                    new PlayerInfoDto(goalkeepers, fieldPlayers)
                );

                personTeams.Add(teamDto);
            }

            PersonWithTeamsDto result = new PersonWithTeamsDto(
                GetPersonIdAsInt(person.Id),
                person.FullName,
                CalculateAge(person.BirthDate) ?? 0,
                personTeams,
                totalMatchesPlayed
            );

            return Result<PersonWithTeamsDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting person with teams for PersonId: {PersonId}", request.PersonId);
            return Result<PersonWithTeamsDto>.Failure($"Error retrieving person with teams: {ex.Message}");
        }
    }

    private static int? CalculateAge(DateTime? birthDate)
    {
        if (!birthDate.HasValue)
            return null;
            
        DateTime today = DateTime.UtcNow;
        int age = today.Year - birthDate.Value.Year;
        if (birthDate.Value.Date > today.AddYears(-age)) age--;
        return age;
    }

    private static int GetPersonIdAsInt(Guid personId)
    {
        // Convert GUID to a stable integer representation
        // This is a simple hash-based approach
        return Math.Abs(personId.GetHashCode());
    }

    private static int GetTeamIdAsInt(Guid teamId)
    {
        // Convert GUID to a stable integer representation
        // This is a simple hash-based approach
        return Math.Abs(teamId.GetHashCode());
    }
} 
