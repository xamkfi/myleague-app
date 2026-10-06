using Application.Common;
using Application.Features.Floorball.Teams.Commands;
using Application.Features.Floorball.Teams.DTOs;
using Domain.Constants;
using Domain.Entities.Common;
using Domain.Entities.Floorball.Teams;
using Domain.Enums.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.ValueObjects.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Teams.Handlers;

/// <summary>
/// Ensures a team has the requested number of loan players and roster rows for the competition.
/// Existing loan players are reused before new ones are created.
/// </summary>
public class EnsureFloorballLoanPlayersHandler
    : IRequestHandler<EnsureFloorballLoanPlayersCommand, Result<IReadOnlyList<FloorballLoanPlayerDto>>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFloorballUnitOfWork _floorballUnitOfWork;
    private readonly ILogger<EnsureFloorballLoanPlayersHandler> _logger;

    public EnsureFloorballLoanPlayersHandler(
        IFloorballTeamRepository teamRepository,
        IFloorballPlayerRepository playerRepository,
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IFloorballUnitOfWork floorballUnitOfWork,
        ILogger<EnsureFloorballLoanPlayersHandler> logger)
    {
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _floorballUnitOfWork = floorballUnitOfWork;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<FloorballLoanPlayerDto>>> Handle(
        EnsureFloorballLoanPlayersCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            FloorballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                return Result<IReadOnlyList<FloorballLoanPlayerDto>>.NotFound("FloorballTeam", request.TeamId);
            }

            List<FloorballPlayer> selected = await EnsurePoolAsync(team, request.Count, request.CompetitionId, cancellationToken);
            await _floorballUnitOfWork.SaveChangesAsync(cancellationToken);

            List<FloorballLoanPlayerDto> dtos = new();
            foreach (FloorballPlayer player in selected)
            {
                FloorballTeamPlayer membership = team.Roster.First(row =>
                    row.PlayerId == player.Id && row.CompetitionId == request.CompetitionId);
                if (membership.JerseyNumber is not int jerseyNumber)
                {
                    throw new InvalidOperationException(LoanJerseyNumbers.NoneAvailableMessage);
                }

                dtos.Add(new FloorballLoanPlayerDto(
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

            return Result<IReadOnlyList<FloorballLoanPlayerDto>>.Success(dtos);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex) when (ex.Message == LoanJerseyNumbers.NoneAvailableMessage)
        {
            _logger.LogError(ex, "No free jersey number for floorball loan players on team {TeamId}", request.TeamId);
            return Result<IReadOnlyList<FloorballLoanPlayerDto>>.Failure(LoanJerseyNumbers.NoneAvailableMessage);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to ensure loan players for floorball team {TeamId}", request.TeamId);
            return Result<IReadOnlyList<FloorballLoanPlayerDto>>.Failure("An error occurred while ensuring the loan players.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid loan player request for floorball team {TeamId}", request.TeamId);
            return Result<IReadOnlyList<FloorballLoanPlayerDto>>.Failure("An error occurred while ensuring the loan players.");
        }
    }

    private async Task<List<FloorballPlayer>> EnsurePoolAsync(
        FloorballTeam team,
        int count,
        Guid? competitionId,
        CancellationToken cancellationToken)
    {
        List<FloorballPlayer> pool = await LoadLoanPlayersAsync(team, cancellationToken);
        List<FloorballPlayer> selected = pool.Take(count).ToList();
        foreach (FloorballPlayer player in selected)
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
            FloorballPlayer created = await CreateLoanPlayerAsync(sequence, cancellationToken);
            pool.Add(created);
            selected.Add(created);
            EnsureRosterMembership(team, created, competitionId);
        }

        return selected;
    }

    private async Task<List<FloorballPlayer>> LoadLoanPlayersAsync(FloorballTeam team, CancellationToken cancellationToken)
    {
        List<Guid> playerIds = team.Roster.Select(row => row.PlayerId).Distinct().ToList();
        if (playerIds.Count == 0)
        {
            return new List<FloorballPlayer>();
        }

        Dictionary<Guid, FloorballPlayer> players = await _playerRepository.GetByIdsAsync(playerIds, cancellationToken);
        return players.Values
            .Where(player => player.IsLoanPlayer)
            .OrderBy(player => player.LoanPlayerNumber)
            .ThenBy(player => player.Id)
            .ToList();
    }

    private async Task<FloorballPlayer> CreateLoanPlayerAsync(int sequence, CancellationToken cancellationToken)
    {
        Person person = new(LoanPlayerNames.FirstName, LoanPlayerNames.LastName(sequence));
        await _personRepository.AddAsync(person);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        FloorballPlayer created = new(person.Id, new Position(FloorballPosition.Forward));
        created.MarkAsLoanPlayer(sequence);
        created.SetPerson(person);
        await _playerRepository.AddAsync(created);
        return created;
    }

    private static void EnsureRosterMembership(FloorballTeam team, FloorballPlayer player, Guid? competitionId)
    {
        FloorballTeamPlayer? membership = team.Roster.FirstOrDefault(row =>
            row.PlayerId == player.Id && row.CompetitionId == competitionId);
        int jersey = LoanJerseyNumbers.Resolve(
            team.Roster.Select(row => new LoanJerseyNumbers.RosterJersey(row.PlayerId, row.CompetitionId, row.JerseyNumber)),
            player.Id,
            competitionId);

        if (membership == null)
        {
            team.AddPlayer(player, FloorballPosition.Forward, jerseyNumber: jersey, competitionId: competitionId);
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
