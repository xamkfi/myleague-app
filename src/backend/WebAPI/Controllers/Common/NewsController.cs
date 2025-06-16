using Application.Common;
using Domain.Common;
using Application.DTOs.Common;
using Application.Commands.NewsArticles;
using Application.Queries.NewsArticles;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;
using Application.Commands.Common;

namespace WebAPI.Controllers.Common
{
    /// <summary>
    /// Controller for managing news articles
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class NewsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<NewsController> _logger;

        /// <summary>
        /// Initializes a new instance of the NewsController class
        /// </summary>
        /// <param name="mediator">The mediator for handling commands and queries</param>
        /// <param name="logger">The logger for this controller</param>
        public NewsController(IMediator mediator, ILogger<NewsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all news articles with pagination and filtering
        /// </summary>
        /// <param name="request">Query parameters for pagination and filtering</param>
        /// <returns>Paginated list of news articles</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedApiResponse<NewsArticleListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<NewsArticleListDto>>> GetAllNews([FromQuery] GetNewsArticlesRequest request)
        {
            _logger.LogInformation("Getting all news articles with pagination - Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

            var query = new GetAllNewsArticlesQuery(
                request.Page,
                request.PageSize,
                request.Category,
                request.SportCategory,
                request.Author,
                request.IncludeArchived
            );

            Result<PagedResult<NewsArticleListDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(PaginatedApiResponse<NewsArticleListDto>.SuccessResponse(result.Data, "News articles retrieved successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, PaginatedApiResponse<NewsArticleListDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Get a specific news article by ID
        /// </summary>
        /// <param name="id">The news article ID</param>
        /// <returns>The news article details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<NewsArticleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<NewsArticleDto>>> GetNewsById(Guid id)
        {
            _logger.LogInformation("Getting news article by ID: {NewsId}", id);

            var query = new GetNewsArticleByIdQuery(id);
            Result<NewsArticleDto> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<NewsArticleDto>.SuccessResponse(result.Data, "News article retrieved successfully"));
            }

            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(ApiResponse<NewsArticleDto>.ErrorResponse($"News article with ID '{id}' not found."));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, ApiResponse<NewsArticleDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Create a new news article
        /// </summary>
        /// <param name="request">The news article creation data</param>
        /// <returns>The created news article</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<NewsArticleDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<NewsArticleDto>>> CreateNews([FromBody] CreateNewsArticleRequest request)
        {
            _logger.LogInformation("Creating new news article with title: {Title}", request.Title);

            var command = new CreateNewsArticleCommand(
                request.Title,
                request.MainImage,
                request.ContentHtml,
                request.Summary,
                request.ImageUrls,
                request.Author,
                request.Category,
                request.SportCategory,
                request.Tags
            );

            Result<NewsArticleDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return CreatedAtAction(nameof(GetNewsById), new { id = result.Data.Id }, 
                    ApiResponse<NewsArticleDto>.SuccessResponse(result.Data, "News article created successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return BadRequest(ApiResponse<NewsArticleDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Update an existing news article
        /// </summary>
        /// <param name="id">The news article ID</param>
        /// <param name="request">The news article update data</param>
        /// <returns>The updated news article</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<NewsArticleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<NewsArticleDto>>> UpdateNews(Guid id, [FromBody] UpdateNewsArticleRequest request)
        {
            _logger.LogInformation("Updating news article with ID: {NewsId}", id);

            var command = new UpdateNewsArticleCommand(
                id,
                request.Title,
                request.MainImage,
                request.ContentHtml,
                request.Summary,
                request.ImageUrls,
                request.Author,
                request.Category,
                request.SportCategory,
                request.Tags
            );

            Result<NewsArticleDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<NewsArticleDto>.SuccessResponse(result.Data, "News article updated successfully"));
            }

            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(ApiResponse<NewsArticleDto>.ErrorResponse($"News article with ID '{id}' not found."));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return BadRequest(ApiResponse<NewsArticleDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Archive a news article
        /// </summary>
        /// <param name="id">The news article ID</param>
        /// <returns>Success status</returns>
        [HttpPost("{id:guid}/archive")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> ArchiveNews(Guid id)
        {
            _logger.LogInformation("Archiving news article with ID: {NewsId}", id);

            var command = new ArchiveNewsArticleCommand(id);
            Result<bool> result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(result.Data, "News article archived successfully"));
            }

            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse($"News article with ID '{id}' not found."));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Restore an archived news article
        /// </summary>
        /// <param name="id">The news article ID</param>
        /// <returns>Success status</returns>
        [HttpPost("{id:guid}/restore")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> RestoreNews(Guid id)
        {
            _logger.LogInformation("Restoring news article with ID: {NewsId}", id);

            var command = new RestoreNewsArticleCommand(id);
            Result<bool> result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(result.Data, "News article restored successfully"));
            }

            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse($"News article with ID '{id}' not found."));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Set an image for a news article
        /// </summary>
        /// <param name="id">The news article ID</param>
        /// <param name="request">The image URL to set</param>
        /// <returns>Success status</returns>
        [HttpPost("{id:guid}/image")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> SetNewsImage(Guid id, [FromBody] SetNewsArticleImageRequest request)
        {
            _logger.LogInformation("Setting image for news article with ID: {NewsId}", id);

            var command = new SetNewsArticleImageCommand(id, request.ImageUrl);
            Result<bool> result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(result.Data, "News article image set successfully"));
            }

            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse($"News article with ID '{id}' not found."));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return BadRequest(ApiResponse<bool>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Add a tag to a news article
        /// </summary>
        /// <param name="id">The news article ID</param>
        /// <param name="request">The tag to add</param>
        /// <returns>Success status</returns>
        [HttpPost("{id:guid}/tags")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> AddNewsTag(Guid id, [FromBody] AddNewsArticleTagRequest request)
        {
            _logger.LogInformation("Adding tag '{Tag}' to news article with ID: {NewsId}", request.Tag, id);

            var command = new AddNewsArticleTagCommand(id, request.Tag);
            Result<bool> result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(result.Data, "Tag added to news article successfully"));
            }

            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse($"News article with ID '{id}' not found."));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return BadRequest(ApiResponse<bool>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Remove a tag from a news article
        /// </summary>
        /// <param name="id">The news article ID</param>
        /// <param name="request">The tag to remove</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id:guid}/tags")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveNewsTag(Guid id, [FromBody] RemoveNewsArticleTagRequest request)
        {
            _logger.LogInformation("Removing tag '{Tag}' from news article with ID: {NewsId}", request.Tag, id);

            var command = new RemoveNewsArticleTagCommand(id, request.Tag);
            Result<bool> result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(result.Data, "Tag removed from news article successfully"));
            }

            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse($"News article with ID '{id}' not found."));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return BadRequest(ApiResponse<bool>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Search news articles by search term
        /// </summary>
        /// <param name="request">The search parameters</param>
        /// <returns>List of matching news articles</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<List<NewsArticleListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<NewsArticleListDto>>>> SearchNews([FromQuery] SearchNewsArticlesRequest request)
        {
            _logger.LogInformation("Searching news articles with term: {SearchTerm}", request.SearchTerm);

            var query = new SearchNewsArticlesQuery(request.SearchTerm);
            Result<IEnumerable<NewsArticleListDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                List<NewsArticleListDto> newsArticleList = result.Data.ToList();
                return Ok(ApiResponse<List<NewsArticleListDto>>.SuccessResponse(newsArticleList, "News articles found successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return BadRequest(ApiResponse<List<NewsArticleListDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Get recent news articles
        /// </summary>
        /// <param name="request">The recent news parameters</param>
        /// <returns>List of recent news articles</returns>
        [HttpGet("recent")]
        [ProducesResponseType(typeof(ApiResponse<List<NewsArticleListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<NewsArticleListDto>>>> GetRecentNews([FromQuery] GetRecentNewsArticlesRequest request)
        {
            _logger.LogInformation("Getting recent news articles - Count: {Count}", request.Count);

            var query = new GetRecentNewsArticlesQuery(request.Count, request.IncludeArchived);
            Result<IEnumerable<NewsArticleListDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                List<NewsArticleListDto> newsArticleList = result.Data.ToList();
                return Ok(ApiResponse<List<NewsArticleListDto>>.SuccessResponse(newsArticleList, "Recent news articles retrieved successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, ApiResponse<List<NewsArticleListDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Get news articles by category
        /// </summary>
        /// <param name="category">The category to filter by</param>
        /// <returns>List of news articles in the specified category</returns>
        [HttpGet("category/{category}")]
        [ProducesResponseType(typeof(ApiResponse<List<NewsArticleListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<NewsArticleListDto>>>> GetNewsByCategory(string category)
        {
            _logger.LogInformation("Getting news articles by category: {Category}", category);

            var query = new GetNewsArticlesByCategoryQuery(category);
            Result<IEnumerable<NewsArticleListDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                List<NewsArticleListDto> newsArticleList = result.Data.ToList();
                return Ok(ApiResponse<List<NewsArticleListDto>>.SuccessResponse(newsArticleList, $"News articles in category '{category}' retrieved successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, ApiResponse<List<NewsArticleListDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Get news articles by tag
        /// </summary>
        /// <param name="tag">The tag to filter by</param>
        /// <returns>List of news articles with the specified tag</returns>
        [HttpGet("tag/{tag}")]
        [ProducesResponseType(typeof(ApiResponse<List<NewsArticleListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<NewsArticleListDto>>>> GetNewsByTag(string tag)
        {
            _logger.LogInformation("Getting news articles by tag: {Tag}", tag);

            var query = new GetNewsArticlesByTagQuery(tag);
            Result<IEnumerable<NewsArticleListDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                List<NewsArticleListDto> newsArticleList = result.Data.ToList();
                return Ok(ApiResponse<List<NewsArticleListDto>>.SuccessResponse(newsArticleList, $"News articles with tag '{tag}' retrieved successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, ApiResponse<List<NewsArticleListDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Get news articles by author
        /// </summary>
        /// <param name="author">The author to filter by</param>
        /// <returns>List of news articles by the specified author</returns>
        [HttpGet("author/{author}")]
        [ProducesResponseType(typeof(ApiResponse<List<NewsArticleListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<NewsArticleListDto>>>> GetNewsByAuthor(string author)
        {
            _logger.LogInformation("Getting news articles by author: {Author}", author);

            var query = new GetNewsArticlesByAuthorQuery(author);
            Result<IEnumerable<NewsArticleListDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                List<NewsArticleListDto> newsArticleList = result.Data.ToList();
                return Ok(ApiResponse<List<NewsArticleListDto>>.SuccessResponse(newsArticleList, $"News articles by author '{author}' retrieved successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, ApiResponse<List<NewsArticleListDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Get all available news categories
        /// </summary>
        /// <returns>List of available news categories</returns>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(ApiResponse<List<NewsArticleCategoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<NewsArticleCategoryDto>>>> GetNewsCategories()
        {
            _logger.LogInformation("Getting all news categories");

            var query = new GetNewsArticleCategoriesQuery();
            Result<IEnumerable<NewsArticleCategoryDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                List<NewsArticleCategoryDto> categories = result.Data.ToList();
                return Ok(ApiResponse<List<NewsArticleCategoryDto>>.SuccessResponse(categories, "News categories retrieved successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, ApiResponse<List<NewsArticleCategoryDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Get all used tags in news articles
        /// </summary>
        /// <returns>List of all used tags</returns>
        [HttpGet("tags")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetNewsTags()
        {
            _logger.LogInformation("Getting all news tags");

            var query = new GetNewsArticleTagsQuery();
            Result<IEnumerable<string>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                List<string> tags = result.Data.ToList();
                return Ok(ApiResponse<List<string>>.SuccessResponse(tags, "News tags retrieved successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, ApiResponse<List<string>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Upload an image and get its URL
        /// </summary>
        /// <param name="file">The image file to upload</param>
        /// <returns>The URL of the uploaded image</returns>
        [HttpPost("upload-image")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<string>>> UploadImage([FromForm] IFormFile file)
        {
            _logger.LogInformation("Uploading image: {FileName}", file?.FileName);

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Image upload failed: No file provided");
                return BadRequest(ApiResponse<string>.ErrorResponse("No file provided"));
            }

            // Validate file type
            string[] allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
            {
                _logger.LogWarning("Image upload failed: Invalid file type {ContentType}", file.ContentType);
                return BadRequest(ApiResponse<string>.ErrorResponse($"Invalid file type. Allowed types: {string.Join(", ", allowedContentTypes)}"));
            }

            // Validate file size (e.g., max 10MB)
            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            if (file.Length > maxFileSize)
            {
                _logger.LogWarning("Image upload failed: File too large {FileSize} bytes", file.Length);
                return BadRequest(ApiResponse<string>.ErrorResponse($"File too large. Maximum size is {maxFileSize / (1024 * 1024)}MB"));
            }

            try
            {
                using Stream stream = file.OpenReadStream();
                
                var command = new UploadImageCommand(
                    stream,
                    file.FileName,
                    file.ContentType);

                Result<Uri> result = await _mediator.Send(command);

                if (result.IsSuccess && result.Data != null)
                {
                    _logger.LogInformation("Image uploaded successfully: {ImageUrl}", result.Data);
                    return Ok(ApiResponse<string>.SuccessResponse(result.Data.ToString(), "Image uploaded successfully"));
                }

                string errorMessage = result.Error ?? result.GetErrorsString();
                _logger.LogError("Image upload failed: {Error}", errorMessage);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(errorMessage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during image upload");
                return StatusCode(500, ApiResponse<string>.ErrorResponse("An unexpected error occurred during image upload"));
            }
        }


        /// <summary>
        /// Delete image from azure blob storage using its URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [HttpDelete("delete-image")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<string>>> DeleteImage([FromQuery] string url)
        {
            _logger.LogInformation("Deleting image: {url}", url);

            if (url == null)
            {
                _logger.LogWarning("Image deletion failed: No url provided");
                return BadRequest(ApiResponse<string>.ErrorResponse("No url provided"));
            }
            
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? imageUri))
            {
                _logger.LogError("Failed to parse URL: '{url}'", url);
                return BadRequest(ApiResponse<string>.ErrorResponse($"Invalid URL format: {url}"));
            }

            _logger.LogInformation("Successfully parsed Uri: {parsedUri}", imageUri);
            try
            {
                DeleteImageCommand command = new DeleteImageCommand(imageUri);

                Result<bool> result = await _mediator.Send(command);

                if (result.IsSuccess && result.Data == true)
                {
                    _logger.LogInformation("Image deleted successfully");
                    return Ok(ApiResponse<string>.SuccessResponse("Image deleted successfully"));
                }

                string errorMessage = result.Error ?? result.GetErrorsString();
                _logger.LogError("Image deletion failed: {Error}", errorMessage);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(errorMessage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during image deletion");
                return StatusCode(500, ApiResponse<string>.ErrorResponse("An unexpected error occurred during image deletion"));
            }
        }
    }
}
