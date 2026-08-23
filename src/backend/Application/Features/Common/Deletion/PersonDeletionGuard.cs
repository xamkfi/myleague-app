using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Football;
using Domain.Repositories.Hockey;

namespace Application.Features.Common.Deletion;

/// <summary>
/// Evaluates person hard-delete against users, club admins, and sport history.
/// </summary>
public sealed class PersonDeletionGuard : IPersonDeletionGuard
{
    private readonly IUserRepository _userRepository;
    private readonly IClubManagerRepository _clubManagerRepository;
    private readonly IFloorballPlayerRepository _floorballPlayerRepository;
    private readonly IFootballPlayerRepository _footballPlayerRepository;
    private readonly IHockeyPlayerRepository _hockeyPlayerRepository;
    private readonly IFloorballRefereeRepository _floorballRefereeRepository;
    private readonly IFootballRefereeRepository _footballRefereeRepository;
    private readonly IHockeyOfficialRepository _hockeyOfficialRepository;
    private readonly IFloorballTeamManagerRepository _floorballTeamManagerRepository;
    private readonly IFootballTeamManagerRepository _footballTeamManagerRepository;

    public PersonDeletionGuard(
        IUserRepository userRepository,
        IClubManagerRepository clubManagerRepository,
        IFloorballPlayerRepository floorballPlayerRepository,
        IFootballPlayerRepository footballPlayerRepository,
        IHockeyPlayerRepository hockeyPlayerRepository,
        IFloorballRefereeRepository floorballRefereeRepository,
        IFootballRefereeRepository footballRefereeRepository,
        IHockeyOfficialRepository hockeyOfficialRepository,
        IFloorballTeamManagerRepository floorballTeamManagerRepository,
        IFootballTeamManagerRepository footballTeamManagerRepository)
    {
        _userRepository = userRepository;
        _clubManagerRepository = clubManagerRepository;
        _floorballPlayerRepository = floorballPlayerRepository;
        _footballPlayerRepository = footballPlayerRepository;
        _hockeyPlayerRepository = hockeyPlayerRepository;
        _floorballRefereeRepository = floorballRefereeRepository;
        _footballRefereeRepository = footballRefereeRepository;
        _hockeyOfficialRepository = hockeyOfficialRepository;
        _floorballTeamManagerRepository = floorballTeamManagerRepository;
        _footballTeamManagerRepository = footballTeamManagerRepository;
    }

    public async Task<PersonDeletionEvaluation> EvaluateAsync(Guid personId, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByPersonIdAsync(personId))
        {
            return new PersonDeletionEvaluation { BlockReason = DeletionReasons.PersonHasUserAccount };
        }

        IEnumerable<Domain.Entities.Common.ClubManager> clubManagers =
            await _clubManagerRepository.GetAllByPersonIdAsync(personId)
            ?? Array.Empty<Domain.Entities.Common.ClubManager>();
        if (clubManagers.Any())
        {
            return new PersonDeletionEvaluation { BlockReason = DeletionReasons.PersonIsClubManager };
        }

        Domain.Entities.Floorball.FloorballPlayer? floorballPlayer =
            await _floorballPlayerRepository.GetByPersonIdAsync(personId);
        if (floorballPlayer != null &&
            await _floorballPlayerRepository.HasCompetitionHistoryAsync(floorballPlayer.Id, cancellationToken))
        {
            return new PersonDeletionEvaluation { BlockReason = DeletionReasons.PersonHasMatchRecords };
        }

        Domain.Entities.Football.Teams.FootballPlayer? footballPlayer =
            await _footballPlayerRepository.GetByPersonIdAsync(personId);
        if (footballPlayer != null &&
            await _footballPlayerRepository.HasCompetitionHistoryAsync(footballPlayer.Id, cancellationToken))
        {
            return new PersonDeletionEvaluation { BlockReason = DeletionReasons.PersonHasMatchRecords };
        }

        Domain.Entities.Hockey.Teams.HockeyPlayer? hockeyPlayer =
            await _hockeyPlayerRepository.GetByPersonIdAsync(personId);
        if (hockeyPlayer != null &&
            await _hockeyPlayerRepository.HasCompetitionHistoryAsync(hockeyPlayer.Id, cancellationToken))
        {
            return new PersonDeletionEvaluation { BlockReason = DeletionReasons.PersonHasMatchRecords };
        }

        Domain.Entities.Floorball.FloorballReferee? floorballReferee =
            await _floorballRefereeRepository.GetByPersonIdAsync(personId);
        if (floorballReferee != null &&
            await _floorballRefereeRepository.IsAssignedToAnyMatchAsync(floorballReferee.Id, cancellationToken))
        {
            return new PersonDeletionEvaluation { BlockReason = DeletionReasons.PersonIsAssignedOfficial };
        }

        Domain.Entities.Football.Teams.FootballReferee? footballReferee =
            await _footballRefereeRepository.GetByPersonIdAsync(personId);
        if (footballReferee != null &&
            await _footballRefereeRepository.IsAssignedToAnyMatchAsync(footballReferee.Id, cancellationToken))
        {
            return new PersonDeletionEvaluation { BlockReason = DeletionReasons.PersonIsAssignedOfficial };
        }

        Domain.Entities.Hockey.Teams.HockeyOfficial? hockeyOfficial =
            await _hockeyOfficialRepository.GetByPersonIdAsync(personId);
        if (hockeyOfficial != null &&
            await _hockeyOfficialRepository.IsAssignedToAnyMatchAsync(hockeyOfficial.Id, cancellationToken))
        {
            return new PersonDeletionEvaluation { BlockReason = DeletionReasons.PersonIsAssignedOfficial };
        }

        IReadOnlyCollection<Guid> floorballManagerIds =
            (await _floorballTeamManagerRepository.GetAllByPersonIdAsync(personId)
                ?? Array.Empty<Domain.Entities.Floorball.FloorballTeamManager>())
            .Select(manager => manager.Id)
            .ToArray();
        IReadOnlyCollection<Guid> footballManagerIds =
            (await _footballTeamManagerRepository.GetAllByPersonIdAsync(personId)
                ?? Array.Empty<Domain.Entities.Football.Teams.FootballTeamManager>())
            .Select(manager => manager.Id)
            .ToArray();

        return new PersonDeletionEvaluation
        {
            UnusedFloorballPlayerId = floorballPlayer?.Id,
            UnusedFootballPlayerId = footballPlayer?.Id,
            UnusedHockeyPlayerId = hockeyPlayer?.Id,
            UnusedFloorballRefereeId = floorballReferee?.Id,
            UnusedFootballRefereeId = footballReferee?.Id,
            UnusedHockeyOfficialId = hockeyOfficial?.Id,
            FloorballTeamManagerIds = floorballManagerIds,
            FootballTeamManagerIds = footballManagerIds
        };
    }
}
