using Application.Common;
using Application.Features.Common.SiteSettings.Commands;
using Application.Features.Common.SiteSettings.DTOs;
using Application.Features.Common.SiteSettings.Queries;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common;

/// <summary>
/// System-admin site settings (auth timings).
/// </summary>
[Route("api/site-settings")]
[Authorize(Roles = AuthRoles.AdminOnly)]
public class SiteSettingsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<SiteSettingsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SiteSettingsController"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR mediator.</param>
    /// <param name="logger">The logger.</param>
    public SiteSettingsController(IMediator mediator, ILogger<SiteSettingsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get effective site settings (persisted row or appsettings fallback).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SiteSettingsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SiteSettingsDto>>> Get()
    {
        _logger.LogInformation("Getting site settings");
        Result<SiteSettingsDto> result = await _mediator.Send(new GetSiteSettingsQuery());
        return HandleResult(result, "Site settings retrieved successfully", "Failed to retrieve site settings");
    }

    /// <summary>
    /// Create or update site settings. New values apply to newly issued tokens and login codes.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<SiteSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SiteSettingsDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SiteSettingsDto>>> Update(
        [FromBody] UpdateSiteSettingsRequest request)
    {
        _logger.LogInformation("Updating site settings");

        UpdateSiteSettingsCommand command = new(
            request.AccessTokenExpirationMinutes,
            request.RefreshTokenExpirationDays,
            request.LoginCodeExpirationMinutes,
            request.LoginCodeMaxAttempts,
            request.SessionExpiryWarningMinutes);

        Result<SiteSettingsDto> result = await _mediator.Send(command);
        return HandleResult(result, "Site settings updated successfully", "Failed to update site settings");
    }
}
