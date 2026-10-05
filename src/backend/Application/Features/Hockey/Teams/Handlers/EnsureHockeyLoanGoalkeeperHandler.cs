using Application.Common;
using Application.Features.Hockey.Teams.Commands;
using Application.Features.Hockey.Teams.DTOs;
using Domain.Constants;
using Domain.Entities.Common;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Teams;
using Domain.Repositories.Common;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Teams.Handlers;

/// <summary>
/// Ensures a team has one loan goalkeeper and an active roster row for the requested competition.
/// </summary>
public class EnsureHockeyLoanGoalkeeperHandler
    : IRequestHandler<EnsureHockeyLoanGoalkeeperCommand, Result<HockeyLoanGoalkeeperDto>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHockeyUnitOfWork _hockeyUnitOfWork;
    private readonly ILogger<EnsureHockeyLoanGoalkeeperHandler> _logger;

    public EnsureHockeyLoanGoalkeeperHandler(
        IHockeyTeamRepository teamRepository,
        IHockeyPlayerRepository playerRepository,
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IHockeyUnitOfWork hockeyUnitOfWork,
        ILogger<EnsureHockeyLoanGoalkeeperHandler> logger)
    {
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _hockeyUnitOfWork = hockeyUnitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyLoanGoalkeeperDto>> Handle(
        EnsureHockeyLoanGoalkeeperCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                return Result<HockeyLoanGoalkeeperDto>.NotFound("HockeyTeam", request.TeamId);
            }

            HockeyPlayer player = await FindOrCreatePlayerAsync(team, cancellationToken);
            EnsureRosterMembership(team, player, request.CompetitionId);
            await _hockeyUnitOfWork.SaveChangesAsync(cancellationToken);

            HockeyTeamPlayer membership = team.Roster.First(row =>
                row.PlayerId == player.Id &&
                row.CompetitionId == request.CompetitionId &&
                row.IsActive);

            return Result<HockeyLoanGoalkeeperDto>.Success(new HockeyLoanGoalkeeperDto(
                player.Id,
                membership.Id,
                LoanGoalkeeperNames.FirstName,
                LoanGoalkeeperNames.LastName,
                LoanGoalkeeperNames.DisplayName,
                HockeyPosition.Goalie,
                player.IsLoanGoalkeeper));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to ensure loan goalkeeper for hockey team {TeamId}", request.TeamId);
            return Result<HockeyLoanGoalkeeperDto>.Failure("An error occurred while ensuring the loan goalkeeper.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid loan goalkeeper request for hockey team {TeamId}", request.TeamId);
            return Result<HockeyLoanGoalkeeperDto>.Failure("An error occurred while ensuring the loan goalkeeper.");
        }
    }

    private async Task<HockeyPlayer> FindOrCreatePlayerAsync(HockeyTeam team, CancellationToken cancellationToken)
    {
        List<Guid> playerIds = team.Roster.Select(row => row.PlayerId).Distinct().ToList();
        if (playerIds.Count > 0)
        {
            Dictionary<Guid, HockeyPlayer> players = await _playerRepository.GetByIdsAsync(playerIds, cancellationToken);
            HockeyPlayer? existing = players.Values.FirstOrDefault(player => player.IsLoanGoalkeeper);
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

        HockeyPlayer created = new(person.Id, HockeyPosition.Goalie);
        created.MarkAsLoanGoalkeeper();
        created.SetPerson(person);
        await _playerRepository.AddAsync(created);
        return created;
    }

    private static void EnsureRosterMembership(HockeyTeam team, HockeyPlayer player, Guid? competitionId)
    {
        HockeyTeamPlayer? membership = team.Roster.FirstOrDefault(row =>
            row.PlayerId == player.Id && row.CompetitionId == competitionId && row.IsActive);

        if (membership == null)
        {
            team.AddPlayer(
                player,
                HockeyPosition.Goalie,
                competitionId,
                rosterStatus: HockeyRosterStatus.Active);
            return;
        }

        if (membership.Position != HockeyPosition.Goalie || membership.RosterStatus != HockeyRosterStatus.Active)
        {
            team.UpdateTeamPlayer(
                player.Id,
                HockeyPosition.Goalie,
                membership.JerseyNumber,
                HockeyRosterStatus.Active,
                membership.CaptainRole,
                competitionId);
        }
    }
}
