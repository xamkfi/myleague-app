using Application.Common;
using Application.Features.Football.Teams.Commands;
using Application.Features.Football.Teams.DTOs;
using Domain.Constants;
using Domain.Entities.Common;
using Domain.Entities.Football.Teams;
using Domain.Enums.Football;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using Domain.ValueObjects.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Teams.Handlers;

/// <summary>
/// Ensures a team has the requested number of loan players and roster rows for the competition.
/// Existing loan players are reused before new ones are created.
/// </summary>
public class EnsureFootballLoanPlayersHandler
    : IRequestHandler<EnsureFootballLoanPlayersCommand, Result<IReadOnlyList<FootballLoanPlayerDto>>>
{
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFootballUnitOfWork _footballUnitOfWork;
    private readonly ILogger<EnsureFootballLoanPlayersHandler> _logger;

    public EnsureFootballLoanPlayersHandler(
        IFootballTeamRepository teamRepository,
        IFootballPlayerRepository playerRepository,
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IFootballUnitOfWork footballUnitOfWork,
        ILogger<EnsureFootballLoanPlayersHandler> logger)
    {
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _footballUnitOfWork = footballUnitOfWork;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<FootballLoanPlayerDto>>> Handle(
        EnsureFootballLoanPlayersCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            FootballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                return Result<IReadOnlyList<FootballLoanPlayerDto>>.NotFound("FootballTeam", request.TeamId);
            }

            List<FootballPlayer> selected = await EnsurePoolAsync(team, request.Count, request.CompetitionId, cancellationToken);
            await _footballUnitOfWork.SaveChangesAsync(cancellationToken);

            List<FootballLoanPlayerDto> dtos = new();
            foreach (FootballPlayer player in selected)
            {
                FootballTeamPlayer membership = team.Roster.First(row =>
                    row.PlayerId == player.Id && row.CompetitionId == request.CompetitionId);
                if (membership.JerseyNumber is not int jerseyNumber)
                {
                    throw new InvalidOperationException(LoanJerseyNumbers.NoneAvailableMessage);
                }

                dtos.Add(new FootballLoanPlayerDto(
                    player.Id,
                    membership.Id,
                    LoanPlayerNames.FirstName,
                    LoanPlayerNames.LastName(player.LoanPlayerNumber),
                    LoanPlayerNames.DisplayName(player.LoanPlayerNumber),
                    jerseyNumber,
                    player.LoanPlayerNumber,
                    membership.Position,
                    player.IsLoanPlayer));
            }

            return Result<IReadOnlyList<FootballLoanPlayerDto>>.Success(dtos);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex) when (ex.Message == LoanJerseyNumbers.NoneAvailableMessage)
        {
            _logger.LogError(ex, "No free jersey number for football loan players on team {TeamId}", request.TeamId);
            return Result<IReadOnlyList<FootballLoanPlayerDto>>.Failure(LoanJerseyNumbers.NoneAvailableMessage);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to ensure loan players for football team {TeamId}", request.TeamId);
            return Result<IReadOnlyList<FootballLoanPlayerDto>>.Failure("An error occurred while ensuring the loan players.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid loan player request for football team {TeamId}", request.TeamId);
            return Result<IReadOnlyList<FootballLoanPlayerDto>>.Failure("An error occurred while ensuring the loan players.");
        }
    }

    private async Task<List<FootballPlayer>> EnsurePoolAsync(
        FootballTeam team,
        int count,
        Guid? competitionId,
        CancellationToken cancellationToken)
    {
        List<FootballPlayer> pool = await LoadLoanPlayersAsync(team, cancellationToken);
        List<FootballPlayer> selected = pool.Take(count).ToList();
        foreach (FootballPlayer player in selected)
        {
            if (!player.IsActive)
            {
                player.UpdateActiveStatus(true);
            }

            EnsureRosterMembership(team, player, competitionId);
        }

        while (selected.Count < count)
        {
            if (LoanJerseyNumbers.FreeCount(team.Roster.Where(row => row.JerseyNumber.HasValue).Select(row => row.JerseyNumber!.Value)) < 1)
            {
                throw new InvalidOperationException(LoanJerseyNumbers.NoneAvailableMessage);
            }

            int sequence = pool.Count == 0 ? 1 : pool.Max(player => player.LoanPlayerNumber) + 1;
            FootballPlayer created = await CreateLoanPlayerAsync(sequence, cancellationToken);
            pool.Add(created);
            selected.Add(created);
            EnsureRosterMembership(team, created, competitionId);
        }

        return selected;
    }

    private async Task<List<FootballPlayer>> LoadLoanPlayersAsync(FootballTeam team, CancellationToken cancellationToken)
    {
        List<Guid> playerIds = team.Roster.Select(row => row.PlayerId).Distinct().ToList();
        if (playerIds.Count == 0)
        {
            return new List<FootballPlayer>();
        }

        Dictionary<Guid, FootballPlayer> players = await _playerRepository.GetByIdsAsync(playerIds, cancellationToken);
        return players.Values
            .Where(player => player.IsLoanPlayer)
            .OrderBy(player => player.LoanPlayerNumber)
            .ThenBy(player => player.Id)
            .ToList();
    }

    private async Task<FootballPlayer> CreateLoanPlayerAsync(int sequence, CancellationToken cancellationToken)
    {
        Person person = new(LoanPlayerNames.FirstName, LoanPlayerNames.LastName(sequence));
        await _personRepository.AddAsync(person);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        FootballPlayer created = new(person.Id, new FootballPositionPreference(FootballPosition.Midfielder));
        created.MarkAsLoanPlayer(sequence);
        created.SetPerson(person);
        await _playerRepository.AddAsync(created);
        return created;
    }

    private static void EnsureRosterMembership(FootballTeam team, FootballPlayer player, Guid? competitionId)
    {
        FootballTeamPlayer? membership = team.Roster.FirstOrDefault(row =>
            row.PlayerId == player.Id && row.CompetitionId == competitionId);
        int jersey = LoanJerseyNumbers.Resolve(
            team.Roster.Select(row => new LoanJerseyNumbers.RosterJersey(row.PlayerId, row.CompetitionId, row.JerseyNumber)),
            player.Id,
            competitionId);

        if (membership == null)
        {
            team.AddPlayer(player, FootballPosition.Midfielder, jerseyNumber: jersey, competitionId: competitionId);
            return;
        }

        if (!membership.IsActive || membership.JerseyNumber != jersey)
        {
            team.UpdateTeamPlayer(
                player.Id,
                membership.Position,
                jersey,
                isActive: true,
                competitionId);
        }
    }
}
