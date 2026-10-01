using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Mappings;
using Application.Features.Hockey.Matches.Queries;
using Domain.Entities.Hockey.Matches;
using Domain.Repositories.Common;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Matches.Handlers;

/// <summary>
/// Handles GetHockeyMatchById.
/// </summary>
public class GetHockeyMatchByIdHandler : IRequestHandler<GetHockeyMatchByIdQuery, Result<HockeyMatchDto>>
{
    private readonly IHockeyMatchRepository _matchRepository;
    private readonly IHockeyOfficialRepository _officialRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetHockeyMatchByIdHandler> _logger;

    public GetHockeyMatchByIdHandler(
        IHockeyMatchRepository matchRepository,
        IHockeyOfficialRepository officialRepository,
        IPersonRepository personRepository,
        ILogger<GetHockeyMatchByIdHandler> logger)
    {
        _matchRepository = matchRepository;
        _officialRepository = officialRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    public async Task<Result<HockeyMatchDto>> Handle(GetHockeyMatchByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            HockeyMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match is null)
            {
                return Result<HockeyMatchDto>.NotFound("HockeyMatch", request.MatchId);
            }

            (Dictionary<Guid, string> officialNames, Dictionary<Guid, string> scorekeeperNames) =
                await HockeyMatchStaffNames.LoadAsync(match, _officialRepository, _personRepository);

            return Result<HockeyMatchDto>.Success(HockeyMatchMapper.ToDto(match, officialNames, scorekeeperNames));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed GetHockeyMatchById for {MatchId}", request.MatchId);
            return Result<HockeyMatchDto>.Failure("An error occurred while retrieving the hockey match.", ex.Flatten());
        }
    }
}
