using Application.Common;
using Application.Features.Common.Organization.DataSubjectRights.Commands;
using Application.Features.Common.Organization.DataSubjectRights.DTOs;
using Application.Features.Common.Organization.DataSubjectRights.Queries;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common;

/// <summary>
/// Admin fulfillment of data-subject rights: access, rectification, erasure, restriction, objection, and portability.
/// </summary>
[Route("api/data-subject-rights")]
[Authorize(Roles = AuthRoles.AdminOnly)]
public class DataSubjectRightsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<DataSubjectRightsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSubjectRightsController"/> class.
    /// </summary>
    public DataSubjectRightsController(IMediator mediator, ILogger<DataSubjectRightsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Returns whether the person's data is processed and a copy of that data.
    /// The portable subset is the data the person supplied.
    /// </summary>
    [HttpGet("{personId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DataSubjectCopyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DataSubjectCopyDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DataSubjectCopyDto>>> GetCopy(Guid personId)
    {
        _logger.LogInformation("Getting data-subject copy for person {PersonId}", personId);
        Result<DataSubjectCopyDto> result = await _mediator.Send(new GetDataSubjectCopyQuery(personId));
        return HandleResult(result, "Data-subject copy retrieved successfully", "Person not found");
    }

    /// <summary>
    /// Corrects inaccurate or incomplete identity data.
    /// </summary>
    [HttpPut("{personId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DataSubjectCopyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DataSubjectCopyDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<DataSubjectCopyDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DataSubjectCopyDto>>> Rectify(Guid personId, [FromBody] RectifyDataSubjectRequest request)
    {
        _logger.LogInformation("Rectifying data-subject data for person {PersonId}", personId);
        RectifyDataSubjectCommand command = new RectifyDataSubjectCommand(
            personId,
            request.FirstName,
            request.LastName,
            request.BirthDate,
            request.Address,
            request.ContactInfo,
            request.AccountEmail);
        Result<DataSubjectCopyDto> result = await _mediator.Send(command);
        return HandleResult(result, "Data-subject data rectified successfully", "Failed to rectify data-subject data");
    }

    /// <summary>
    /// Erases identity data where processing no longer has a basis. League records stay under an anonymized name.
    /// </summary>
    [HttpPost("{personId:guid}/erasure")]
    [ProducesResponseType(typeof(ApiResponse<DataSubjectCopyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DataSubjectCopyDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<DataSubjectCopyDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DataSubjectCopyDto>>> Erase(Guid personId)
    {
        _logger.LogInformation("Erasing data-subject data for person {PersonId}", personId);
        Result<DataSubjectCopyDto> result = await _mediator.Send(new EraseDataSubjectCommand(personId));
        return HandleResult(result, "Data-subject data erased successfully", "Failed to erase data-subject data");
    }

    /// <summary>
    /// Restricts processing or lifts a restriction. Restriction suspends the linked account.
    /// </summary>
    [HttpPut("{personId:guid}/restriction")]
    [ProducesResponseType(typeof(ApiResponse<DataSubjectCopyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DataSubjectCopyDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<DataSubjectCopyDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DataSubjectCopyDto>>> SetRestriction(
        Guid personId,
        [FromBody] SetDataSubjectFlagRequest request)
    {
        _logger.LogInformation("Setting data-subject restriction for person {PersonId} to {IsRestricted}", personId, request.Enabled);
        Result<DataSubjectCopyDto> result = await _mediator.Send(new SetDataSubjectRestrictionCommand(personId, request.Enabled));
        return HandleResult(result, "Data-subject restriction updated successfully", "Failed to update data-subject restriction");
    }

    /// <summary>
    /// Records or withdraws an objection to legitimate-interest processing.
    /// </summary>
    [HttpPut("{personId:guid}/objection")]
    [ProducesResponseType(typeof(ApiResponse<DataSubjectCopyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DataSubjectCopyDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<DataSubjectCopyDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DataSubjectCopyDto>>> SetObjection(
        Guid personId,
        [FromBody] SetDataSubjectFlagRequest request)
    {
        _logger.LogInformation("Setting data-subject objection for person {PersonId} to {HasObjected}", personId, request.Enabled);
        Result<DataSubjectCopyDto> result = await _mediator.Send(new SetDataSubjectObjectionCommand(personId, request.Enabled));
        return HandleResult(result, "Data-subject objection updated successfully", "Failed to update data-subject objection");
    }
}
