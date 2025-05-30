using Application.Commands.Clubs;
using Application.Commands.Persons;
using Application.Common;
using Application.DTOs.Common;
using Application.Queries.Persons;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;
using Domain.ValueObjects.Common;
using Application.Mappings.Common;

namespace WebAPI.Controllers.Common
{
    /// <summary>
    /// Controller for managing persons
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PersonsController : ControllerBase
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
        [ProducesResponseType(typeof(ApiResponse<List<PersonDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<PersonDto>>>> GetAllPersons()
        {
            _logger.LogInformation("Getting all persons");

            GetAllPersonsQuery query = new GetAllPersonsQuery();
            Result<IEnumerable<PersonDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                List<PersonDto> personList = result.Data.ToList();
                return Ok(ApiResponse<List<PersonDto>>.SuccessResponse(personList, "Persons retrieved successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, ApiResponse<List<PersonDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Get person by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PersonDto>>> GetPersonById(Guid id)
        {
            _logger.LogInformation("Getting person by Id: {Id}", id);

            GetPersonByIdQuery query = new GetPersonByIdQuery(id);
            Result<PersonDto> result = await _mediator.Send(query);

            if(result.IsSuccess && result.Data != null)
            {
                PersonDto person = result.Data;
                return Ok(ApiResponse<PersonDto>.SuccessResponse(person, "Person retrieved successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            
            // Check if it's a not found error
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<PersonDto>.ErrorResponse(errorMessage));
            }
            
            return StatusCode(500, ApiResponse<PersonDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Get person by email
        /// </summary>
        /// <param name="email">The email address to search for</param>
        /// <returns></returns>
        [HttpGet("by-email")]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PersonDto>>> GetPersonByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(ApiResponse<PersonDto>.ErrorResponse("Email parameter is required"));
            }

            _logger.LogInformation("Getting person by email: {Email}", email);

            GetPersonByEmailQuery query = new GetPersonByEmailQuery(email);
            Result<PersonDto> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                PersonDto person = result.Data;
                return Ok(ApiResponse<PersonDto>.SuccessResponse(person, "Person retrieved successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            
            // Check if it's a not found error
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<PersonDto>.ErrorResponse(errorMessage));
            }
            
            return StatusCode(500, ApiResponse<PersonDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Search persons by name
        /// </summary>
        /// <param name="name">The name to search for (searches both first and last names)</param>
        /// <returns></returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<List<PersonDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<PersonDto>>>> SearchPersonsByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(ApiResponse<List<PersonDto>>.ErrorResponse("Name parameter is required"));
            }

            _logger.LogInformation("Searching persons by name: {Name}", name);

            SearchPersonByNameQuery query = new SearchPersonByNameQuery(name);
            Result<IEnumerable<PersonDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                List<PersonDto> personList = result.Data.ToList();
                return Ok(ApiResponse<List<PersonDto>>.SuccessResponse(personList, $"Found {personList.Count} persons matching '{name}'"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, ApiResponse<List<PersonDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Create a new person
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PersonDto>>> CreatePerson([FromBody] CreatePersonRequest request)
        {
            _logger.LogInformation("Creating new person: {FirstName} {LastName}", request.FirstName, request.LastName);

            CreatePersonCommand command = new CreatePersonCommand(
                request.FirstName,
                request.LastName,
                request.BirthDate,
                request.Address,
                request.ContactInfo);

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
        /// Update an existing person completely
        /// </summary>
        /// <param name="id">The person ID</param>
        /// <param name="request">The update request</param>
        /// <returns></returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PersonDto>>> UpdatePerson(Guid id, [FromBody] UpdatePersonRequest request)
        {
            _logger.LogInformation("Updating person with Id: {Id}", id);

            UpdatePersonCommand command = new UpdatePersonCommand(
                id,
                request.FirstName,
                request.LastName,
                request.BirthDate,
                request.Address,
                request.ContactInfo);

            Result<PersonDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<PersonDto>.SuccessResponse(result.Data, "Person updated successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            
            // Check if it's a not found error
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<PersonDto>.ErrorResponse(errorMessage));
            }
            
            return BadRequest(ApiResponse<PersonDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Update person's basic information (first name and last name)
        /// </summary>
        /// <param name="id">The person ID</param>
        /// <param name="request">The basic info update request</param>
        /// <returns></returns>
        [HttpPatch("{id:guid}/basic-info")]
        [ProducesResponseType(typeof(ApiResponse<PersonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PersonDto>>> UpdatePersonBasicInfo(Guid id, [FromBody] UpdatePersonBasicInfoRequest request)
        {
            _logger.LogInformation("Updating person basic info with Id: {Id}", id);

            UpdatePersonBasicInfoCommand command = new UpdatePersonBasicInfoCommand(id, request.FirstName, request.LastName);
            Result<PersonDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<PersonDto>.SuccessResponse(result.Data, "Person basic information updated successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            
            // Check if it's a not found error
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<PersonDto>.ErrorResponse(errorMessage));
            }
            
            return BadRequest(ApiResponse<PersonDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Update person's address
        /// </summary>
        /// <param name="id">The person ID</param>
        /// <param name="request">The address update request</param>
        /// <returns></returns>
        [HttpPatch("{id:guid}/address")]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
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

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<AddressDto>.SuccessResponse(result.Data, "Person address updated successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            
            // Check if it's a not found error
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<AddressDto>.ErrorResponse(errorMessage));
            }
            
            return BadRequest(ApiResponse<AddressDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Update person's contact information
        /// </summary>
        /// <param name="id">The person ID</param>
        /// <param name="request">The contact info update request</param>
        /// <returns></returns>
        [HttpPatch("{id:guid}/contact-info")]
        [ProducesResponseType(typeof(ApiResponse<ContactInfoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ContactInfoDto>>> UpdatePersonContactInfo(Guid id, [FromBody] UpdatePersonContactInfoRequest request)
        {
            _logger.LogInformation("Updating person contact info with Id: {Id}", id);

            ContactInfo contactInfo = new ContactInfo(
                request.Email,
                request.Phone,
                request.AlternativePhone);

            UpdatePersonContactInfoCommand command = new UpdatePersonContactInfoCommand(id, contactInfo);
            Result<ContactInfoDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<ContactInfoDto>.SuccessResponse(result.Data, "Person contact information updated successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            
            // Check if it's a not found error
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<ContactInfoDto>.ErrorResponse(errorMessage));
            }
            
            return BadRequest(ApiResponse<ContactInfoDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Delete an existing person
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeletePerson(Guid id)
        {
            _logger.LogInformation("Deleting person with Id: {Id}", id);

            DeletePersonCommand command = new DeletePersonCommand(id);
            Result result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse.SuccessResponse("Person deleted successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();

            // Check if it's a not found error
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse.ErrorResponse(errorMessage));
        }
    }
}
