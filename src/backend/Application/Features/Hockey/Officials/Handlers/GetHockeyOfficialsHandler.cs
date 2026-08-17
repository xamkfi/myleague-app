using Application.Common;
using Application.Features.Hockey.Officials.DTOs;
using Application.Features.Hockey.Officials.Mappings;
using Application.Features.Hockey.Officials.Queries;
using Domain.Entities.Hockey.Teams;
using Domain.Repositories.Hockey;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Hockey.Officials.Handlers;

/// <summary>
/// Handles listing hockey officials.
/// </summary>
public class GetHockeyOfficialsHandler
    : IRequestHandler<GetHockeyOfficialsQuery, Result<IReadOnlyList<HockeyOfficialDto>>>
{
    private readonly IHockeyOfficialRepository _officialRepository;
    private readonly ILogger<GetHockeyOfficialsHandler> _logger;

    public GetHockeyOfficialsHandler(
        IHockeyOfficialRepository officialRepository,
        ILogger<GetHockeyOfficialsHandler> logger)
    {
        _officialRepository = officialRepository;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<HockeyOfficialDto>>> Handle(
        GetHockeyOfficialsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<HockeyOfficial> officials = await _officialRepository.GetAllAsync(request.IsActive);
            IReadOnlyList<HockeyOfficialDto> dtos = officials.Select(HockeyOfficialMapper.ToDto).ToList();
            return Result<IReadOnlyList<HockeyOfficialDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed GetHockeyOfficials");
            return Result<IReadOnlyList<HockeyOfficialDto>>.Failure(
                "An error occurred while listing hockey officials.",
                ex.Flatten());
        }
    }
}
