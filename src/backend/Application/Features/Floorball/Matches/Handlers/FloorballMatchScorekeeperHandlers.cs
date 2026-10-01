using Application.Common;
using Application.Features.Common.Shared;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Mappings;
using Domain.Entities.Common;
using Domain.Entities.Floorball.Matches;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handles <see cref="AddFloorballScorekeeperToMatchCommand"/>.
/// </summary>
public class AddFloorballScorekeeperToMatchHandler : IRequestHandler<AddFloorballScorekeeperToMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<AddFloorballScorekeeperToMatchHandler> _logger;

    public AddFloorballScorekeeperToMatchHandler(
        IFloorballMatchRepository matchRepository,
        IPersonRepository personRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<AddFloorballScorekeeperToMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(AddFloorballScorekeeperToMatchCommand request, CancellationToken cancellationToken)
    {
        FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match is null)
            return Result<FloorballMatchDto>.NotFound("FloorballMatch", request.MatchId);

        if (!await _personRepository.ExistsAsync(request.PersonId))
            return Result<FloorballMatchDto>.NotFound("Person", request.PersonId);

        try
        {
            match.AddScorekeeper(request.PersonId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot add scorekeeper {PersonId} to match {MatchId}", request.PersonId, request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Dictionary<Guid, Person> staff = await MatchPersonLookup.LoadAsync(
            _personRepository, FloorballMatchMapper.CollectStaffPersonIds(match));
        return Result<FloorballMatchDto>.Success(
            FloorballMatchMapper.ToDto(match, new Dictionary<Guid, Person>(), staffPersonLookup: staff));
    }
}

/// <summary>
/// Handles <see cref="RemoveFloorballScorekeeperFromMatchCommand"/>.
/// </summary>
public class RemoveFloorballScorekeeperFromMatchHandler : IRequestHandler<RemoveFloorballScorekeeperFromMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveFloorballScorekeeperFromMatchHandler> _logger;

    public RemoveFloorballScorekeeperFromMatchHandler(
        IFloorballMatchRepository matchRepository,
        IPersonRepository personRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<RemoveFloorballScorekeeperFromMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(RemoveFloorballScorekeeperFromMatchCommand request, CancellationToken cancellationToken)
    {
        FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match is null)
            return Result<FloorballMatchDto>.NotFound("FloorballMatch", request.MatchId);

        try
        {
            match.RemoveScorekeeper(request.PersonId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot remove scorekeeper {PersonId} from match {MatchId}", request.PersonId, request.MatchId);
            return Result<FloorballMatchDto>.Failure(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Dictionary<Guid, Person> staff = await MatchPersonLookup.LoadAsync(
            _personRepository, FloorballMatchMapper.CollectStaffPersonIds(match));
        return Result<FloorballMatchDto>.Success(
            FloorballMatchMapper.ToDto(match, new Dictionary<Guid, Person>(), staffPersonLookup: staff));
    }
}
