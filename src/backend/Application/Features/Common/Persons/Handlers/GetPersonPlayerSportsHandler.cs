using Application.Common;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Persons.Queries;
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

namespace Application.Features.Common.Persons.Handlers;

/// <summary>
/// Resolves a person or sport-player id and lists linked sport profiles.
/// </summary>
public class GetPersonPlayerSportsHandler : IRequestHandler<GetPersonPlayerSportsQuery, Result<PersonPlayerSportsDto>>
{
    private readonly IPersonRepository _personRepository;
    private readonly IFloorballPlayerRepository _floorballPlayerRepository;
    private readonly IFootballPlayerRepository _footballPlayerRepository;
    private readonly IHockeyPlayerRepository _hockeyPlayerRepository;
    private readonly ILogger<GetPersonPlayerSportsHandler> _logger;

    public GetPersonPlayerSportsHandler(
        IPersonRepository personRepository,
        IFloorballPlayerRepository floorballPlayerRepository,
        IFootballPlayerRepository footballPlayerRepository,
        IHockeyPlayerRepository hockeyPlayerRepository,
        ILogger<GetPersonPlayerSportsHandler> logger)
    {
        _personRepository = personRepository;
        _floorballPlayerRepository = floorballPlayerRepository;
        _footballPlayerRepository = footballPlayerRepository;
        _hockeyPlayerRepository = hockeyPlayerRepository;
        _logger = logger;
    }

    public async Task<Result<PersonPlayerSportsDto>> Handle(
        GetPersonPlayerSportsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting player sports for Id: {Id}", request.Id);

        Person? person = await _personRepository.GetByIdAsync(request.Id);
        if (person == null)
        {
            Guid? personId = await ResolvePersonIdFromSportPlayerAsync(request.Id);
            if (personId.HasValue)
            {
                person = await _personRepository.GetByIdAsync(personId.Value);
            }
        }

        if (person == null)
        {
            _logger.LogWarning("Person or player with ID {Id} not found", request.Id);
            return Result<PersonPlayerSportsDto>.NotFound("Person", request.Id);
        }

        FloorballPlayer? floorballPlayer = await _floorballPlayerRepository.GetByPersonIdAsync(person.Id);
        FootballPlayer? footballPlayer = await _footballPlayerRepository.GetByPersonIdAsync(person.Id);
        HockeyPlayer? hockeyPlayer = await _hockeyPlayerRepository.GetByPersonIdAsync(person.Id);

        List<PersonSportPlayerDto> sports = new();
        if (floorballPlayer != null)
        {
            sports.Add(new PersonSportPlayerDto("floorball", floorballPlayer.Id));
        }

        if (footballPlayer != null)
        {
            sports.Add(new PersonSportPlayerDto("football", footballPlayer.Id));
        }

        if (hockeyPlayer != null)
        {
            sports.Add(new PersonSportPlayerDto("hockey", hockeyPlayer.Id));
        }

        return Result<PersonPlayerSportsDto>.Success(new PersonPlayerSportsDto(
            person.Id,
            person.FullName,
            person.BirthDate,
            sports));
    }

    private async Task<Guid?> ResolvePersonIdFromSportPlayerAsync(Guid id)
    {
        FloorballPlayer? floorballPlayer = await _floorballPlayerRepository.GetByIdAsync(id);
        if (floorballPlayer != null)
        {
            return floorballPlayer.PersonId;
        }

        FootballPlayer? footballPlayer = await _footballPlayerRepository.GetByIdAsync(id);
        if (footballPlayer != null)
        {
            return footballPlayer.PersonId;
        }

        HockeyPlayer? hockeyPlayer = await _hockeyPlayerRepository.GetByIdAsync(id);
        if (hockeyPlayer != null)
        {
            return hockeyPlayer.PersonId;
        }

        return null;
    }
}
