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
/// Ensures a team has one loan goalkeeper and a roster row for the requested competition.
/// </summary>
public class EnsureFloorballLoanGoalkeeperHandler
    : IRequestHandler<EnsureFloorballLoanGoalkeeperCommand, Result<FloorballLoanGoalkeeperDto>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFloorballUnitOfWork _floorballUnitOfWork;
    private readonly ILogger<EnsureFloorballLoanGoalkeeperHandler> _logger;

    public EnsureFloorballLoanGoalkeeperHandler(
        IFloorballTeamRepository teamRepository,
        IFloorballPlayerRepository playerRepository,
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IFloorballUnitOfWork floorballUnitOfWork,
        ILogger<EnsureFloorballLoanGoalkeeperHandler> logger)
    {
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _floorballUnitOfWork = floorballUnitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballLoanGoalkeeperDto>> Handle(
        EnsureFloorballLoanGoalkeeperCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            FloorballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                return Result<FloorballLoanGoalkeeperDto>.NotFound("FloorballTeam", request.TeamId);
            }

            FloorballPlayer player = await FindOrCreatePlayerAsync(team, cancellationToken);
            EnsureRosterMembership(team, player, request.CompetitionId);
            await _floorballUnitOfWork.SaveChangesAsync(cancellationToken);

            FloorballTeamPlayer membership = team.Roster.First(row =>
                row.PlayerId == player.Id && row.CompetitionId == request.CompetitionId);

            return Result<FloorballLoanGoalkeeperDto>.Success(new FloorballLoanGoalkeeperDto(
                player.Id,
                membership.Id,
                LoanGoalkeeperNames.FirstName,
                LoanGoalkeeperNames.LastName,
                LoanGoalkeeperNames.DisplayName,
                FloorballPosition.Goalkeeper,
                player.IsLoanGoalkeeper));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to ensure loan goalkeeper for floorball team {TeamId}", request.TeamId);
            return Result<FloorballLoanGoalkeeperDto>.Failure("An error occurred while ensuring the loan goalkeeper.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid loan goalkeeper request for floorball team {TeamId}", request.TeamId);
            return Result<FloorballLoanGoalkeeperDto>.Failure("An error occurred while ensuring the loan goalkeeper.");
        }
    }

    private async Task<FloorballPlayer> FindOrCreatePlayerAsync(FloorballTeam team, CancellationToken cancellationToken)
    {
        List<Guid> playerIds = team.Roster.Select(row => row.PlayerId).Distinct().ToList();
        if (playerIds.Count > 0)
        {
            Dictionary<Guid, FloorballPlayer> players = await _playerRepository.GetByIdsAsync(playerIds, cancellationToken);
            FloorballPlayer? existing = players.Values.FirstOrDefault(player => player.IsLoanGoalkeeper);
            if (existing != null)
            {
                if (!existing.IsActive)
                {
                    existing.UpdateActiveStatus(true);
                }

                return existing;
            }
        }

        Person person = new(LoanGoalkeeperNames.FirstName, LoanGoalkeeperNames.LastName);
        await _personRepository.AddAsync(person);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        FloorballPlayer created = new(person.Id, new Position(FloorballPosition.Goalkeeper));
        created.MarkAsLoanGoalkeeper();
        created.SetPerson(person);
        await _playerRepository.AddAsync(created);
        return created;
    }

    private static void EnsureRosterMembership(FloorballTeam team, FloorballPlayer player, Guid? competitionId)
    {
        FloorballTeamPlayer? membership = team.Roster.FirstOrDefault(row =>
            row.PlayerId == player.Id && row.CompetitionId == competitionId);

        if (membership == null)
        {
            team.AddPlayer(player, FloorballPosition.Goalkeeper, jerseyNumber: null, competitionId: competitionId);
            return;
        }

        if (!membership.IsActive || membership.Position != FloorballPosition.Goalkeeper)
        {
            team.UpdateTeamPlayer(
                player.Id,
                FloorballPosition.Goalkeeper,
                membership.JerseyNumber,
                isActive: true,
                competitionId);
        }
    }
}
