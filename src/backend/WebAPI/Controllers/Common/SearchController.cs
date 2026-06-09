using Application.Common;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.Search.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common
{
    /// <summary>
    /// Controller for handling global search operations across multiple entity types.
    /// </summary>
    [Route("api/[controller]")]
    public class SearchController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SearchController> _logger;

        /// <summary>
        /// Initializes a new instance of the SearchController class.
        /// </summary>
        /// <param name="mediator">The mediator for handling commands and queries.</param>
        /// <param name="logger">The logger for this controller.</param>
        public SearchController(IMediator mediator, ILogger<SearchController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Performs a global search across clubs, players, and teams.
        /// </summary>
        /// <param name="term">The search term.</param>
        /// <returns>A combined list of search results.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<GlobalSearchResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<GlobalSearchResultDto>>> GlobalSearch([FromQuery] string term)
        {
            _logger.LogInformation("Performing global search with term: {SearchTerm}", term);

            Result<GlobalSearchResultDto> result = await _mediator.Send(new GlobalSearchQuery(term));

            return HandleResult(result, "Search completed successfully", "Search failed");
        }
    }
}
