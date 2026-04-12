using System.Security.Claims;
using Application.Common;
using Application.Features.Common.SiteSettings.Commands;
using Application.Features.Common.SiteSettings.DTOs;
using Application.Features.Common.SiteSettings.Queries;
using Domain.Enums.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common;

/// <summary>
/// Controller for site-level settings APIs.
/// </summary>
[ApiController]
[Route("api/site-settings")]
[Produces("application/json")]
public class SiteSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SiteSettingsController"/> class.
    /// </summary>
    public SiteSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets public footer contact settings.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("footer-contact")]
    [ProducesResponseType(typeof(ApiResponse<FooterContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FooterContactDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FooterContactDto>>> GetFooterContact(CancellationToken cancellationToken)
    {
        Result<FooterContactDto> result = await _mediator.Send(new GetFooterContactQuery(), cancellationToken);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<FooterContactDto>.SuccessResponse(result.Data, "Footer contact settings retrieved successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        return StatusCode(500, ApiResponse<FooterContactDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Updates footer contact settings.
    /// </summary>
    [Authorize(Roles = $"{nameof(UserRole.SystemAdmin)},Admin")]
    [HttpPut("footer-contact")]
    [ProducesResponseType(typeof(ApiResponse<FooterContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FooterContactDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<FooterContactDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<FooterContactDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<FooterContactDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FooterContactDto>>> UpdateFooterContact(
        [FromBody] UpdateFooterContactRequest request,
        CancellationToken cancellationToken)
    {
        string modifiedBy = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "Admin";

        var command = new UpdateFooterContactCommand(
            request.OrganizationName,
            request.OrganizationAddress,
            request.ContactPersons.Select(x => new FooterContactPersonUpdateDto(x.NameOrRole, x.Email, x.Phone)).ToList(),
            modifiedBy);

        Result<FooterContactDto> result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<FooterContactDto>.SuccessResponse(result.Data, "Footer contact settings updated successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        List<string> errorList = result.ValidationFailures.Select(x => x.ErrorMessage).ToList();

        return BadRequest(ApiResponse<FooterContactDto>.ErrorResponse(errorMessage, errorList));
    }
}
