using Application.Queries.Clubs;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Application.Common;
using Domain.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System.Linq;

namespace Application.Handlers.Clubs;

/// <summary>
/// Handler for retrieving clubs with pagination
/// </summary>
public class GetClubsPagedHandler : IRequestHandler<GetClubsPagedQuery, Result<PagedResult<ClubDto>>>
{
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetClubsPagedHandler> _logger;

    public GetClubsPagedHandler(IClubRepository clubRepository, ILogger<GetClubsPagedHandler> logger)
    {
        _clubRepository = clubRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ClubDto>>> Handle(GetClubsPagedQuery request, CancellationToken cancellationToken)
    {
        try
        {
            int page = request.Page <= 0 ? 1 : request.Page;
            int pageSize = request.PageSize <= 0 ? 50 : request.PageSize;

            IEnumerable<Domain.Entities.Common.Club> clubs = await _clubRepository.GetAllAsync();
            List<Domain.Entities.Common.Club> ordered = clubs.OrderBy(c => c.Name).ToList();

            int totalCount = ordered.Count;
            List<Domain.Entities.Common.Club> pageItems = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            IEnumerable<ClubDto> dtos = ClubMapper.ToDtos(pageItems);
            PagedResult<ClubDto> paged = PagedResult.Create(dtos, totalCount, page, pageSize);

            return Result<PagedResult<ClubDto>>.Success(paged);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving paged clubs");
            return Result<PagedResult<ClubDto>>.Failure("An error occurred while retrieving clubs.");
        }
    }
}


