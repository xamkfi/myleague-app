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
/// Ensures a team has one loan goalkeeper and a roster row for the requested competition.
/// </summary>
public class EnsureFootballLoanGoalkeeperHandler
    : IRequestHandler<EnsureFootballLoanGoalkeeperCommand, Result<FootballLoanGoalkeeperDto>>
{
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFootballUnitOfWork _footballUnitOfWork;
    private readonly ILogger<EnsureFootballLoanGoalkeeperHandler> _logger;

    public EnsureFootballLoanGoalkeeperHandler(
        IFootballTeamRepository teamRepository,
        IFootballPlayerRepository playerRepository,
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IFootballUnitOfWork footballUnitOfWork,
        ILogger<EnsureFootballLoanGoalkeeperHandler> logger)
    {
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _footballUnitOfWork = footballUnitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballLoanGoalkeeperDto>> Handle(
        EnsureFootballLoanGoalkeeperCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            FootballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                return Result<FootballLoanGoalkeeperDto>.NotFound("FootballTeam", request.TeamId);
            }

            FootballPlayer player = await FindOrCreatePlayerAsync(team, cancellationToken);
            EnsureRosterMembership(team, player, request.CompetitionId);
            await _footballUnitOfWork.SaveChangesAsync(cancellationToken);

            FootballTeamPlayer membership = team.Roster.First(row =>
                row.PlayerId == player.Id && row.CompetitionId == request.CompetitionId);

            return Result<FootballLoanGoalkeeperDto>.Success(new FootballLoanGoalkeeperDto(
                player.Id,
                membership.Id,
                LoanGoalkeeperNames.FirstName,
                LoanGoalkeeperNames.LastName,
                LoanGoalkeeperNames.DisplayName,
                FootballPosition.Goalkeeper,
                player.IsLoanGoalkeeper));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to ensure loan goalkeeper for football team {TeamId}", request.TeamId);
            return Result<FootballLoanGoalkeeperDto>.Failure("An error occurred while ensuring the loan goalkeeper.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid loan goalkeeper request for football team {TeamId}", request.TeamId);
            return Result<FootballLoanGoalkeeperDto>.Failure("An error occurred while ensuring the loan goalkeeper.");
        }
    }

    private async Task<FootballPlayer> FindOrCreatePlayerAsync(FootballTeam team, CancellationToken cancellationToken)
    {
        List<Guid> playerIds = team.Roster.Select(row => row.PlayerId).Distinct().ToList();
        if (playerIds.Count > 0)
        {
            Dictionary<Guid, FootballPlayer> players = await _playerRepository.GetByIdsAsync(playerIds, cancellationToken);
            FootballPlayer? existing = players.Values.FirstOrDefault(player => player.IsLoanGoalkeeper);
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

        FootballPlayer created = new(person.Id, new FootballPositionPreference(FootballPosition.Goalkeeper));
        created.MarkAsLoanGoalkeeper();
        created.SetPerson(person);
        await _playerRepository.AddAsync(created);
        return created;
    }

    private static void EnsureRosterMembership(FootballTeam team, FootballPlayer player, Guid? competitionId)
    {
        FootballTeamPlayer? membership = team.Roster.FirstOrDefault(row =>
            row.PlayerId == player.Id && row.CompetitionId == competitionId);

        if (membership == null)
        {
            team.AddPlayer(player, FootballPosition.Goalkeeper, jerseyNumber: null, competitionId: competitionId);
            return;
        }

        if (!membership.IsActive || membership.Position != FootballPosition.Goalkeeper)
        {
            team.UpdateTeamPlayer(
                player.Id,
                FootballPosition.Goalkeeper,
                membership.JerseyNumber,
                isActive: true,
                competitionId);
        }
    }
}
