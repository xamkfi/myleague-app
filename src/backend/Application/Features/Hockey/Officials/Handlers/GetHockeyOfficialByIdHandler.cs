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
/// Handles getting a hockey official by id.
/// </summary>
public class GetHockeyOfficialByIdHandler : IRequestHandler<GetHockeyOfficialByIdQuery, Result<HockeyOfficialDto>>
{
    private readonly IHockeyOfficialRepository _officialRepository;
    private readonly ILogger<GetHockeyOfficialByIdHandler> _logger;

    public GetHockeyOfficialByIdHandler(
        IHockeyOfficialRepository officialRepository,
        ILogger<GetHockeyOfficialByIdHandler> logger)
    {
        _officialRepository = officialRepository;
        _logger = logger;
    }

    public async Task<Result<HockeyOfficialDto>> Handle(
        GetHockeyOfficialByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            HockeyOfficial? official = await _officialRepository.GetByIdAsync(request.OfficialId);
            if (official is null)
            {
                return Result<HockeyOfficialDto>.NotFound("HockeyOfficial", request.OfficialId);
            }

            return Result<HockeyOfficialDto>.Success(HockeyOfficialMapper.ToDto(official));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed GetHockeyOfficialById for {OfficialId}", request.OfficialId);
            return Result<HockeyOfficialDto>.Failure(
                "An error occurred while retrieving the hockey official.",
                ex.Flatten());
        }
    }
}
