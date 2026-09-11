using Application.Common;
using Application.Features.Common.SiteSettings.DTOs;
using Application.Features.Common.SiteSettings.Mappings;
using Application.Interfaces.Common;
using MediatR;

namespace Application.Features.Common.SiteSettings.Queries;

public record GetSiteSettingsQuery : IRequest<Result<SiteSettingsDto>>;

public class GetSiteSettingsQueryHandler
    : IRequestHandler<GetSiteSettingsQuery, Result<SiteSettingsDto>>
{
    private readonly ISiteSettingsProvider _siteSettingsProvider;

    public GetSiteSettingsQueryHandler(ISiteSettingsProvider siteSettingsProvider)
    {
        _siteSettingsProvider = siteSettingsProvider;
    }

    public async Task<Result<SiteSettingsDto>> Handle(
        GetSiteSettingsQuery request,
        CancellationToken cancellationToken)
    {
        EffectiveAuthSettings settings = await _siteSettingsProvider.GetEffectiveAsync(cancellationToken);
        return Result<SiteSettingsDto>.Success(SiteSettingsMapper.ToDto(settings));
    }
}
