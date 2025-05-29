using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Commands.Persons;
using Application.Common;
using Application.DTOs.Common;
using Application.Queries.Persons;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common;

/// <summary>
/// Controller for managing persons
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PersonsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PersonsController> _logger;

    /// <summary>
    /// Initializes a new instance of the PersonsController class
    /// </summary>
    /// <param name="mediator">The mediator instance</param>
    /// <param name="logger">The logger instance</param>
    public PersonsController(IMediator mediator, ILogger<PersonsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all persons
    /// </summary>
    /// <returns>List of persons</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PersonDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PersonDto>>>> GetAllPersons()
    {
        _logger.LogInformation("Getting all persons");
        
        GetAllPersonsQuery query = new GetAllPersonsQuery();
        Result<IEnumerable<PersonDto>> result = await _mediator.Send(query);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<IEnumerable<PersonDto>>.SuccessResponse(result.Data, "Persons retrieved successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        return StatusCode(500, ApiResponse.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Get a person by ID
    /// </summary>
    /// <param name="id">Person ID</param>
    /// <returns>Person details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PersonDto>>> GetPersonById(Guid id)
    {
        _logger.LogInformation("Getting person with ID: {PersonId}", id);
        
        GetPersonByIdQuery query = new GetPersonByIdQuery(id);
        Result<PersonDto> result = await _mediator.Send(query);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<PersonDto>.SuccessResponse(result.Data, "Person retrieved successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse<PersonDto>.ErrorResponse(errorMessage));
        }

        return StatusCode(500, ApiResponse<PersonDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Create a new person
    /// </summary>
    /// <param name="request">Person creation request</param>
    /// <returns>Created person details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PersonDto>>> CreatePerson([FromBody] CreatePersonRequest request)
    {
        _logger.LogInformation("Creating new person: {FirstName} {LastName}", request.FirstName, request.LastName);

        var address = new AddressDto(
            request.Address.Street1,
            request.Address.Street2 ?? "",
            request.Address.City,
            request.Address.PostalCode,
            request.Address.Country
        );

        var contactInfo = new ContactInfoDto(
            request.ContactInfo.Email,
            request.ContactInfo.Phone,
            request.ContactInfo.AlternativePhone
        );

        CreatePersonCommand command = new CreatePersonCommand(
            request.FirstName,
            request.LastName,
            request.BirthDate,
            address,
            contactInfo
        );

        Result<PersonDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetPersonById),
                new { id = result.Data.Id },
                ApiResponse<PersonDto>.SuccessResponse(result.Data, "Person created successfully")
            );
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        return BadRequest(ApiResponse<PersonDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Update an existing person
    /// </summary>
    /// <param name="id">Person ID</param>
    /// <param name="request">Person update request</param>
    /// <returns>Updated person details</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PersonDto>>> UpdatePerson(Guid id, [FromBody] UpdatePersonRequest request)
    {
        _logger.LogInformation("Updating person with ID: {PersonId}", id);

        var address = new AddressDto(
            request.Address.Street1,
            request.Address.Street2 ?? "",
            request.Address.City,
            request.Address.PostalCode,
            request.Address.Country
        );

        var contactInfo = new ContactInfoDto(
            request.ContactInfo.Email,
            request.ContactInfo.Phone,
            request.ContactInfo.AlternativePhone
        );

        UpdatePersonCommand command = new UpdatePersonCommand(
            id,
            request.FirstName,
            request.LastName,
            request.BirthDate,
            address,
            contactInfo
        );

        Result<PersonDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<PersonDto>.SuccessResponse(result.Data, "Person updated successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse<PersonDto>.ErrorResponse(errorMessage));
        }

        return BadRequest(ApiResponse<PersonDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Delete a person
    /// </summary>
    /// <param name="id">Person ID</param>
    /// <returns>Success or error response</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> DeletePerson(Guid id)
    {
        _logger.LogInformation("Deleting person with ID: {PersonId}", id);

        DeletePersonCommand command = new DeletePersonCommand(id);
        Result result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse.SuccessResponse("Person deleted successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse.ErrorResponse(errorMessage));
        }

        return StatusCode(500, ApiResponse.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Search persons by name
    /// </summary>
    /// <param name="name">Name to search for</param>
    /// <returns>List of matching persons</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PersonDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PersonDto>>>> SearchPersonsByName([FromQuery] string name)
    {
        _logger.LogInformation("Searching persons with name containing: {Name}", name);

        SearchPersonByNameQuery query = new SearchPersonByNameQuery(name);
        Result<IEnumerable<PersonDto>> result = await _mediator.Send(query);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<IEnumerable<PersonDto>>.SuccessResponse(result.Data, "Persons retrieved successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse<IEnumerable<PersonDto>>.ErrorResponse(errorMessage));
        }

        return StatusCode(500, ApiResponse<IEnumerable<PersonDto>>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Get a person by email
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>Person details</returns>
    [HttpGet("by-email")]
    [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PersonDto>>> GetPersonByEmail([FromQuery] string email)
    {
        _logger.LogInformation("Getting person with email: {Email}", email);

        GetPersonByEmailQuery query = new GetPersonByEmailQuery(email);
        Result<PersonDto> result = await _mediator.Send(query);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<PersonDto>.SuccessResponse(result.Data, "Person retrieved successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        return NotFound(ApiResponse<PersonDto>.ErrorResponse(errorMessage));
    }
} 