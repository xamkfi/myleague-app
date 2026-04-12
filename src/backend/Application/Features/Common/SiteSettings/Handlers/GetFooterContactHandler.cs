using Application.Common;
using Application.Features.Common.SiteSettings.DTOs;
using Application.Features.Common.SiteSettings.Mappings;
using Application.Features.Common.SiteSettings.Queries;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Features.Common.SiteSettings.Handlers;

/// <summary>
/// Handles footer contact settings fetch requests.
/// </summary>
public class GetFooterContactHandler : IRequestHandler<GetFooterContactQuery, Result<FooterContactDto>>
{
    private const string FooterContactKey = "footer-contact";

    private readonly ISiteSettingRepository _repository;
    private readonly ILogger<GetFooterContactHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFooterContactHandler"/> class.
    /// </summary>
    public GetFooterContactHandler(ISiteSettingRepository repository, ILogger<GetFooterContactHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the query and returns footer contact settings.
    /// </summary>
    public async Task<Result<FooterContactDto>> Handle(GetFooterContactQuery request, CancellationToken cancellationToken)
    {
        try
        {
            SiteSetting? setting = await _repository.GetByKeyAsync(FooterContactKey, cancellationToken);

            if (setting == null)
            {
                return Result<FooterContactDto>.Success(new FooterContactDto(
                    string.Empty,
                    string.Empty,
                    null,
                    null,
                    Array.Empty<FooterContactPersonDto>()));
            }

            FooterContactSettingValue? value = JsonSerializer.Deserialize<FooterContactSettingValue>(setting.ValueJson);

            if (value == null)
            {
                return Result<FooterContactDto>.Success(new FooterContactDto(
                    string.Empty,
                    string.Empty,
                    setting.LastModifiedBy,
                    setting.UpdatedAt ?? setting.CreatedAt,
                    Array.Empty<FooterContactPersonDto>()));
            }

            return Result<FooterContactDto>.Success(FooterContactMapper.ToDto(
                value,
                setting.LastModifiedBy,
                setting.UpdatedAt ?? setting.CreatedAt));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving footer contact settings");
            return Result<FooterContactDto>.Failure("An error occurred while retrieving footer contact settings.");
        }
    }
}
