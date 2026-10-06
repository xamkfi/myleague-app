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
/// Ensures a team has the requested number of loan players and roster rows for the competition.
/// Existing loan players are reused before new ones are created.
/// </summary>
public class EnsureHockeyLoanPlayersHandler
    : IRequestHandler<EnsureHockeyLoanPlayersCommand, Result<IReadOnlyList<HockeyLoanPlayerDto>>>
{
    private readonly IHockeyTeamRepository _teamRepository;
    private readonly IHockeyPlayerRepository _playerRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHockeyUnitOfWork _hockeyUnitOfWork;
    private readonly ILogger<EnsureHockeyLoanPlayersHandler> _logger;

    public EnsureHockeyLoanPlayersHandler(
        IHockeyTeamRepository teamRepository,
        IHockeyPlayerRepository playerRepository,
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IHockeyUnitOfWork hockeyUnitOfWork,
        ILogger<EnsureHockeyLoanPlayersHandler> logger)
    {
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _hockeyUnitOfWork = hockeyUnitOfWork;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<HockeyLoanPlayerDto>>> Handle(
        EnsureHockeyLoanPlayersCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                return Result<IReadOnlyList<HockeyLoanPlayerDto>>.NotFound("HockeyTeam", request.TeamId);
            }

            List<HockeyPlayer> selected = await EnsurePoolAsync(team, request.Count, request.CompetitionId, cancellationToken);
            await _hockeyUnitOfWork.SaveChangesAsync(cancellationToken);

            List<HockeyLoanPlayerDto> dtos = new();
            foreach (HockeyPlayer player in selected)
            {
                HockeyTeamPlayer membership = team.Roster.First(row =>
                    row.PlayerId == player.Id &&
                    row.CompetitionId == request.CompetitionId &&
                    row.IsActive);
                if (membership.JerseyNumber is not int jerseyNumber)
                {
                    throw new InvalidOperationException(LoanJerseyNumbers.NoneAvailableMessage);
                }

                dtos.Add(new HockeyLoanPlayerDto(
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

            return Result<IReadOnlyList<HockeyLoanPlayerDto>>.Success(dtos);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex) when (ex.Message == LoanJerseyNumbers.NoneAvailableMessage)
        {
            _logger.LogError(ex, "No free jersey number for hockey loan players on team {TeamId}", request.TeamId);
            return Result<IReadOnlyList<HockeyLoanPlayerDto>>.Failure(LoanJerseyNumbers.NoneAvailableMessage);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to ensure loan players for hockey team {TeamId}", request.TeamId);
            return Result<IReadOnlyList<HockeyLoanPlayerDto>>.Failure("An error occurred while ensuring the loan players.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid loan player request for hockey team {TeamId}", request.TeamId);
            return Result<IReadOnlyList<HockeyLoanPlayerDto>>.Failure("An error occurred while ensuring the loan players.");
        }
    }

    private async Task<List<HockeyPlayer>> EnsurePoolAsync(
        HockeyTeam team,
        int count,
        Guid? competitionId,
        CancellationToken cancellationToken)
    {
        List<HockeyPlayer> pool = await LoadLoanPlayersAsync(team, cancellationToken);
        List<HockeyPlayer> selected = pool.Take(count).ToList();
        foreach (HockeyPlayer player in selected)
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
            HockeyPlayer created = await CreateLoanPlayerAsync(sequence, cancellationToken);
            pool.Add(created);
            selected.Add(created);
            EnsureRosterMembership(team, created, competitionId);
        }

        return selected;
    }

    private async Task<List<HockeyPlayer>> LoadLoanPlayersAsync(HockeyTeam team, CancellationToken cancellationToken)
    {
        List<Guid> playerIds = team.Roster.Select(row => row.PlayerId).Distinct().ToList();
        if (playerIds.Count == 0)
        {
            return new List<HockeyPlayer>();
        }

        Dictionary<Guid, HockeyPlayer> players = await _playerRepository.GetByIdsAsync(playerIds, cancellationToken);
        return players.Values
            .Where(player => player.IsLoanPlayer)
            .OrderBy(player => player.LoanPlayerNumber)
            .ThenBy(player => player.Id)
            .ToList();
    }

    private async Task<HockeyPlayer> CreateLoanPlayerAsync(int sequence, CancellationToken cancellationToken)
    {
        Person person = new(LoanPlayerNames.FirstName, LoanPlayerNames.LastName(sequence));
        await _personRepository.AddAsync(person);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        HockeyPlayer created = new(person.Id, HockeyPosition.Center);
        created.MarkAsLoanPlayer(sequence);
        created.SetPerson(person);
        await _playerRepository.AddAsync(created);
        return created;
    }

    private static void EnsureRosterMembership(HockeyTeam team, HockeyPlayer player, Guid? competitionId)
    {
        HockeyTeamPlayer? membership = team.Roster.FirstOrDefault(row =>
            row.PlayerId == player.Id && row.CompetitionId == competitionId && row.IsActive);
        int jersey = LoanJerseyNumbers.Resolve(
            team.Roster.Select(row => new LoanJerseyNumbers.RosterJersey(row.PlayerId, row.CompetitionId, row.JerseyNumber)),
            player.Id,
            competitionId);

        if (membership == null)
        {
            team.AddPlayer(
                player,
                HockeyPosition.Center,
                competitionId,
                jersey,
                rosterStatus: HockeyRosterStatus.Active);
            return;
        }

        if (membership.JerseyNumber != jersey || membership.RosterStatus != HockeyRosterStatus.Active)
        {
            team.UpdateTeamPlayer(
                player.Id,
                membership.Position,
                jersey,
                HockeyRosterStatus.Active,
                membership.CaptainRole,
                competitionId);
        }
    }
}
