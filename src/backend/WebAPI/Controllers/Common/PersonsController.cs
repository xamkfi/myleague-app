using Domain.Constants;
using Application.Common;
using Application.Features.Common.Clubs.Commands;
using Application.Features.Common.Persons.Commands;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Persons.Queries;
using Application.Features.Common.Shared.DTOs;
using Domain.Common;
using Domain.ValueObjects.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;

namespace WebAPI.Controllers.Common
{
    /// <summary>
    /// Controller for managing persons
    /// </summary>
    [Route("api/[controller]")]
    public class PersonsController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PersonsController> _logger;

        /// <summary>
        /// Initializes a new instance of the PersonController class
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public PersonsController(IMediator mediator, ILogger<PersonsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all persons
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedApiResponse<PersonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<PersonDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<PersonDto>>> GetAllPersons([FromQuery] GetPersonsRequest request)
        {
            _logger.LogInformation("Getting all persons");

            GetAllPersonsQuery query = new GetAllPersonsQuery(
                request.Page,
                request.PageSize,
                request.FirstName,
                request.LastName,
                request.BirthDate,
                request.IsRegistered
                );

            Result<PagedResult<PersonDto>> result = await _mediator.Send(query);

            return HandlePaginatedResult(result, "Persons retrieved successfully", "Failed to retrieve persons");
        }

        /// <summary>
        /// Get person by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PersonDto>>> GetPersonById(Guid id)
        {
            _logger.LogInformation("Getting person by Id: {Id}", id);

            GetPersonByIdQuery query = new GetPersonByIdQuery(id);
            Result<PersonDto> result = await _mediator.Send(query);

            return HandleResult(result, "Person retrieved successfully", "Person not found");
        }

        /// <summary>
        /// Get person by email
        /// </summary>
        /// <param name="email">The email address to search for</param>
        /// <returns></returns>
        [HttpGet("by-email")]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PersonDto>>> GetPersonByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(ApiResponse<PersonDto>.ErrorResponse("Email parameter is required"));
            }

            _logger.LogInformation("Getting person by email: {Email}", SanitizeForLog(email));

            GetPersonByEmailQuery query = new GetPersonByEmailQuery(email);
            Result<PersonDto> result = await _mediator.Send(query);

            return HandleResult(result, "Person retrieved successfully", "Person not found");
        }

        /// <summary>
        /// Search persons by name
        /// </summary>
        /// <param name="name">The name to search for (searches both first and last names)</param>
        /// <param name="page">The page number (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <returns></returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(PaginatedApiResponse<PersonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<PersonDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaginatedApiResponse<PersonDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<PersonDto>>> SearchPersonsByName(
            [FromQuery] string name,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(PaginatedApiResponse<PersonDto>.ErrorResponse("Name parameter is required"));
            }

            _logger.LogInformation("Searching persons by name: {Name} - Page: {Page}, PageSize: {PageSize}", SanitizeForLog(name), page, pageSize);

            SearchPersonByNameQuery query = new SearchPersonByNameQuery(name, page, pageSize);
            Result<PagedResult<PersonDto>> result = await _mediator.Send(query);

            return HandlePaginatedResult(result, $"Found {result.Data?.TotalCount ?? 0} persons matching '{name}'", "Failed to search persons");
        }

        /// <summary>
        /// Create a new person
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PersonDto>>> CreatePerson([FromBody] CreatePersonRequest request)
        {
            _logger.LogInformation(
                "Creating new person: {FirstName} {LastName}",
                SanitizeForLog(request.FirstName),
                SanitizeForLog(request.LastName));

            // Parse BirthDate if provided
            DateTime? birthDateUtc = null;
            if (!string.IsNullOrWhiteSpace(request.BirthDate))
            {
                if (!DateTime.TryParse(request.BirthDate, out DateTime parsedDate))
                    return BadRequest(ApiResponse<PersonDto>.ErrorResponse("Birth date must be a valid date-time in ISO 8601 format (e.g., 2017-07-21T17:32:28Z, 2020.10.25, 2020-10-25)"));
                birthDateUtc = parsedDate;
            }

            CreatePersonCommand command = new CreatePersonCommand(
                request.FirstName,
                request.LastName,
                birthDateUtc,
                request.IsRegistered,
                request.Address,
                request.ContactInfo);

            Result<PersonDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data is not null)
            {
                return CreatedAtAction(
                    nameof(GetPersonById),
                    new { id = result.Data.Id },
                    ApiResponse<PersonDto>.SuccessResponse(result.Data, "Person created successfully")
                );
            }

            return ToErrorResponse(result, "Failed to create person");
        }

        /// <summary>
        /// Update an existing person completely
        /// </summary>
        /// <param name="id">The person ID</param>
        /// <param name="request">The update request</param>
        /// <returns></returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PersonDto>>> UpdatePerson(Guid id, [FromBody] UpdatePersonRequest request)
        {
            _logger.LogInformation("Updating person with Id: {Id}", id);

            UpdatePersonCommand command = new UpdatePersonCommand(
                id,
                request.FirstName,
                request.LastName,
                request.BirthDate,
                request.IsRegistered,
                request.Address,
                request.ContactInfo);

            Result<PersonDto> result = await _mediator.Send(command);

            return HandleResult(result, "Person updated successfully", "Failed to update person");
        }

        /// <summary>
        /// Update person's basic information (first name and last name)
        /// </summary>
        /// <param name="id">The person ID</param>
        /// <param name="request">The basic info update request</param>
        /// <returns></returns>
        [HttpPatch("{id:guid}/basic-info")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PersonDto>>> UpdatePersonBasicInfo(Guid id, [FromBody] UpdatePersonBasicInfoRequest request)
        {
            _logger.LogInformation("Updating person basic info with Id: {Id}", id);

            UpdatePersonBasicInfoCommand command = new UpdatePersonBasicInfoCommand(id, request.FirstName, request.LastName);
            Result<PersonDto> result = await _mediator.Send(command);

            return HandleResult(result, "Person basic information updated successfully", "Failed to update person basic information");
        }

        /// <summary>
        /// Update person's address
        /// </summary>
        /// <param name="id">The person ID</param>
        /// <param name="request">The address update request</param>
        /// <returns></returns>
        [HttpPatch("{id:guid}/address")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AddressDto>>> UpdatePersonAddress(Guid id, [FromBody] UpdatePersonAddressRequest request)
        {
            _logger.LogInformation("Updating person address with Id: {Id}", id);

            Address address = new Address(
                request.Street1,
                request.City,
                request.PostalCode,
                request.Country,
                request.Street2);

            UpdatePersonAddressCommand command = new UpdatePersonAddressCommand(id, address);
            Result<AddressDto> result = await _mediator.Send(command);

            return HandleResult(result, "Person address updated successfully", "Failed to update person address");
        }

        /// <summary>
        /// Update person's contact information
        /// </summary>
        /// <param name="id">The person ID</param>
        /// <param name="request">The contact info update request</param>
        /// <returns></returns>
        [HttpPatch("{id:guid}/contact-info")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<ContactInfoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ContactInfoDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ContactInfoDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ContactInfoDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ContactInfoDto>>> UpdatePersonContactInfo(Guid id, [FromBody] UpdatePersonContactInfoRequest request)
        {
            _logger.LogInformation("Updating person contact info with Id: {Id}", id);

            ContactInfo contactInfo = new ContactInfo(
                request.Email ?? string.Empty,
                request.Phone,
                request.AlternativePhone);

            UpdatePersonContactInfoCommand command = new UpdatePersonContactInfoCommand(id, contactInfo);
            Result<ContactInfoDto> result = await _mediator.Send(command);

            return HandleResult(result, "Person contact information updated successfully", "Failed to update person contact information");
        }

        /// <summary>
        /// Update person's registration status
        /// </summary>
        /// <param name="id">The person ID</param>
        /// <param name="isRegistered">The registration status</param>
        /// <returns></returns>
        [HttpPatch("{id:guid}/registration")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PersonDto>>> UpdatePersonRegistration(Guid id, [FromBody] bool isRegistered)
        {
            _logger.LogInformation("Updating person registration status with Id: {Id} to {IsRegistered}", id, isRegistered);

            UpdatePersonRegistrationCommand command = new UpdatePersonRegistrationCommand(id, isRegistered);
            Result<PersonDto> result = await _mediator.Send(command);

            return HandleResult(result, "Person registration status updated successfully", "Failed to update person registration status");
        }

        /// <summary>
        /// Get person with their teams
        /// </summary>
        /// <param name="id">Person ID</param>
        /// <returns>Person with teams information</returns>
        [HttpGet("{id:guid}/teams")]
        [ProducesResponseType(typeof(ApiResponse<PersonWithTeamsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonWithTeamsDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PersonWithTeamsDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PersonWithTeamsDto>>> GetPersonWithTeams(Guid id)
        {
            _logger.LogInformation("Getting person with teams for Id: {Id}", id);

            GetPersonWithTeamsQuery query = new GetPersonWithTeamsQuery(id);
            Result<PersonWithTeamsDto> result = await _mediator.Send(query);

            return HandleResult(result, "Person with teams retrieved successfully", "Person not found");
        }

        /// <summary>
        /// Update person's role
        /// </summary>
        /// <param name="id">The person ID</param>
        /// <param name="role">The new role</param>
        /// <returns>The updated person</returns>
        [HttpPatch("{id:guid}/role")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PersonDto>>> UpdatePersonRole(Guid id, [FromBody] Domain.Enums.Common.PersonRole role)
        {
            _logger.LogInformation("Updating person role with Id: {Id} to {Role}", id, SanitizeForLog(role));

            UpdatePersonRoleCommand command = new UpdatePersonRoleCommand(id, role);
            Result<PersonDto> result = await _mediator.Send(command);

            return HandleResult(result, "Person role updated successfully", "Failed to update person role");
        }

        /// <summary>
        /// Delete an existing person
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeletePerson(Guid id)
        {
            _logger.LogInformation("Deleting person with Id: {Id}", id);

            DeletePersonCommand command = new DeletePersonCommand(id);
            Result result = await _mediator.Send(command);

            return HandleVoidResult(result, "Person deleted successfully", "Failed to delete person");
        }
    }
}
