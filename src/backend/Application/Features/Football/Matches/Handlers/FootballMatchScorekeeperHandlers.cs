using Application.Common;
using Application.Features.Common.Shared;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Domain.Entities.Common;
using Domain.Entities.Football.Matches;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

/// <summary>
/// Handles <see cref="AddFootballScorekeeperToMatchCommand"/>.
/// </summary>
public class AddFootballScorekeeperToMatchHandler : IRequestHandler<AddFootballScorekeeperToMatchCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<AddFootballScorekeeperToMatchHandler> _logger;

    public AddFootballScorekeeperToMatchHandler(
        IFootballMatchRepository matchRepository,
        IPersonRepository personRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<AddFootballScorekeeperToMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(AddFootballScorekeeperToMatchCommand request, CancellationToken cancellationToken)
    {
        FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match is null)
            return Result<FootballMatchDto>.NotFound("FootballMatch", request.MatchId);

        if (!await _personRepository.ExistsAsync(request.PersonId))
            return Result<FootballMatchDto>.NotFound("Person", request.PersonId);

        try
        {
            match.AddScorekeeper(request.PersonId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot add scorekeeper {PersonId} to match {MatchId}", request.PersonId, request.MatchId);
            return Result<FootballMatchDto>.Failure(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Dictionary<Guid, Person> staff = await MatchPersonLookup.LoadAsync(
            _personRepository, FootballMatchMapper.CollectStaffPersonIds(match));
        return Result<FootballMatchDto>.Success(
            FootballMatchMapper.ToDto(match, new Dictionary<Guid, Person>(), staffPersonLookup: staff));
    }
}

/// <summary>
/// Handles <see cref="RemoveFootballScorekeeperFromMatchCommand"/>.
/// </summary>
public class RemoveFootballScorekeeperFromMatchHandler : IRequestHandler<RemoveFootballScorekeeperFromMatchCommand, Result<FootballMatchDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveFootballScorekeeperFromMatchHandler> _logger;

    public RemoveFootballScorekeeperFromMatchHandler(
        IFootballMatchRepository matchRepository,
        IPersonRepository personRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<RemoveFootballScorekeeperFromMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FootballMatchDto>> Handle(RemoveFootballScorekeeperFromMatchCommand request, CancellationToken cancellationToken)
    {
        FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match is null)
            return Result<FootballMatchDto>.NotFound("FootballMatch", request.MatchId);

        try
        {
            match.RemoveScorekeeper(request.PersonId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot remove scorekeeper {PersonId} from match {MatchId}", request.PersonId, request.MatchId);
            return Result<FootballMatchDto>.Failure(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Dictionary<Guid, Person> staff = await MatchPersonLookup.LoadAsync(
            _personRepository, FootballMatchMapper.CollectStaffPersonIds(match));
        return Result<FootballMatchDto>.Success(
            FootballMatchMapper.ToDto(match, new Dictionary<Guid, Person>(), staffPersonLookup: staff));
    }
}
