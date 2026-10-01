using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Domain.Entities.Hockey.Matches;
using Domain.Repositories.Common;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles <see cref="AddHockeyMatchScorekeeperCommand"/>.
/// </summary>
public class AddHockeyMatchScorekeeperHandler : IRequestHandler<AddHockeyMatchScorekeeperCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyOfficialRepository _officialRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<AddHockeyMatchScorekeeperHandler> _logger;

    public AddHockeyMatchScorekeeperHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyOfficialRepository officialRepository,
        IPersonRepository personRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<AddHockeyMatchScorekeeperHandler> logger)
    {
        _matchRepository = matchRepository;
        _officialRepository = officialRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(AddHockeyMatchScorekeeperCommand request, CancellationToken cancellationToken)
    {
        HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match is null)
            return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);

        if (!await _personRepository.ExistsAsync(request.PersonId))
            return Result<HockeyMatchDto>.NotFound("Person", request.PersonId);

        try
        {
            match.AddScorekeeper(request.PersonId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot add scorekeeper {PersonId} to match {MatchId}", request.PersonId, request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        (Dictionary<Guid, string> officialNames, Dictionary<Guid, string> scorekeeperNames) =
            await HockeyMatchStaffNames.LoadAsync(match, _officialRepository, _personRepository);
        return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match, officialNames, scorekeeperNames));
    }
}

/// <summary>
/// Handles <see cref="RemoveHockeyMatchScorekeeperCommand"/>.
/// </summary>
public class RemoveHockeyMatchScorekeeperHandler : IRequestHandler<RemoveHockeyMatchScorekeeperCommand, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyOfficialRepository _officialRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IHockeyUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveHockeyMatchScorekeeperHandler> _logger;

    public RemoveHockeyMatchScorekeeperHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyOfficialRepository officialRepository,
        IPersonRepository personRepository,
        IHockeyUnitOfWork unitOfWork,
        ILogger<RemoveHockeyMatchScorekeeperHandler> logger)
    {
        _matchRepository = matchRepository;
        _officialRepository = officialRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(RemoveHockeyMatchScorekeeperCommand request, CancellationToken cancellationToken)
    {
        HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match is null)
            return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);

        try
        {
            match.RemoveScorekeeper(request.PersonId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot remove scorekeeper {PersonId} from match {MatchId}", request.PersonId, request.MatchId);
            return Result<HockeyMatchDto>.Failure(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        (Dictionary<Guid, string> officialNames, Dictionary<Guid, string> scorekeeperNames) =
            await HockeyMatchStaffNames.LoadAsync(match, _officialRepository, _personRepository);
        return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match, officialNames, scorekeeperNames));
    }
}
