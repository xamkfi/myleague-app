using Domain.Constants;
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Common;
using Application.Features.Common.Images.Commands;
using Application.Features.Common.News.Commands;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.News.Queries;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;

namespace WebAPI.Controllers.Common
{
    /// <summary>
    /// Controller for managing news articles
    /// </summary>
    [Route("api/[controller]")]
    public class NewsController : BaseApiController
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
        [ProducesResponseType(typeof(PaginatedApiResponse<NewsArticleListDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaginatedApiResponse<NewsArticleListDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<NewsArticleListDto>>> GetAllNews([FromQuery] GetNewsArticlesRequest request)
        {
            _logger.LogInformation("Getting all news articles with pagination - Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

            var query = new GetAllNewsArticlesQuery(
                request.Page,
                request.PageSize,
                request.Category,
                request.SportCategory,
                request.Search,
                request.Author,
                request.IncludeArchived,
                request.TeamCategories
            );

            Result<PagedResult<NewsArticleListDto>> result = await _mediator.Send(query);

            return HandlePaginatedResult(result, "News articles retrieved successfully", "Failed to retrieve news articles");
        }

        /// <summary>
        /// Get a specific news article by ID
        /// </summary>
        /// <param name="id">The news article ID</param>
        /// <returns>The news article details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<NewsArticleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<NewsArticleDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<NewsArticleDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<NewsArticleDto>>> GetNewsById(Guid id)
        {
            _logger.LogInformation("Getting news article by ID: {NewsId}", id);

            var query = new GetNewsArticleByIdQuery(id);
            Result<NewsArticleDto> result = await _mediator.Send(query);

            return HandleResult(result, "News article retrieved successfully", "News article not found");
        }

        /// <summary>
        /// Create a new news article
        /// </summary>
        /// <param name="request">The news article creation data</param>
        /// <returns>The created news article</returns>
        [HttpPost]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<NewsArticleDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<NewsArticleDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<NewsArticleDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<NewsArticleDto>>> CreateNews([FromBody] CreateNewsArticleRequest request)
        {
            _logger.LogInformation("Creating new news article with title: {Title}", SanitizeForLog(request.Title));

            var command = new CreateNewsArticleCommand(
                request.Title,
                request.MainImage,
                request.ContentHtml,
                request.Summary,
                request.ImageUrls,
                request.Author,
                request.Category,
                request.SportCategory,
                request.Tags,
                request.TeamCategory
            );

            Result<NewsArticleDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data is not null)
            {
                return CreatedAtAction(nameof(GetNewsById), new { id = result.Data.Id },
                    ApiResponse<NewsArticleDto>.SuccessResponse(result.Data, "News article created successfully"));
            }

            return ToErrorResponse(result, "Failed to create news article");
        }

        /// <summary>
        /// Update an existing news article
        /// </summary>
        /// <param name="id">The news article ID</param>
        /// <param name="request">The news article update data</param>
        /// <returns>The updated news article</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<NewsArticleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<NewsArticleDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<NewsArticleDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<NewsArticleDto>), StatusCodes.Status500InternalServerError)]
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
                request.Tags,
                request.TeamCategory
            );

            Result<NewsArticleDto> result = await _mediator.Send(command);

            return HandleResult(result, "News article updated successfully", "Failed to update news article");
        }

        /// <summary>
        /// Archive a news article
        /// </summary>
        /// <param name="id">The news article ID</param>
        /// <returns>Success status</returns>
        [HttpPost("{id:guid}/archive")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> ArchiveNews(Guid id)
        {
            _logger.LogInformation("Archiving news article with ID: {NewsId}", id);

            var command = new ArchiveNewsArticleCommand(id);
            Result<bool> result = await _mediator.Send(command);

            return HandleResult(result, "News article archived successfully", "Failed to archive news article");
        }

        /// <summary>
        /// Restore an archived news article
        /// </summary>
        /// <param name="id">The news article ID</param>
        /// <returns>Success status</returns>
        [HttpPost("{id:guid}/restore")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> RestoreNews(Guid id)
        {
            _logger.LogInformation("Restoring news article with ID: {NewsId}", id);

            var command = new RestoreNewsArticleCommand(id);
            Result<bool> result = await _mediator.Send(command);

            return HandleResult(result, "News article restored successfully", "Failed to restore news article");
        }

        /// <summary>
        /// Set an image for a news article
        /// </summary>
        /// <param name="id">The news article ID</param>
        /// <param name="request">The image URL to set</param>
        /// <returns>Success status</returns>
        [HttpPost("{id:guid}/image")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> SetNewsImage(Guid id, [FromBody] SetNewsArticleImageRequest request)
        {
            _logger.LogInformation("Setting image for news article with ID: {NewsId}", id);

            var command = new SetNewsArticleImageCommand(id, request.ImageUrl);
            Result<bool> result = await _mediator.Send(command);

            return HandleResult(result, "News article image set successfully", "Failed to set news article image");
        }

        /// <summary>
        /// Add a tag to a news article
        /// </summary>
        /// <param name="id">The news article ID</param>
        /// <param name="request">The tag to add</param>
        /// <returns>Success status</returns>
        [HttpPost("{id:guid}/tags")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> AddNewsTag(Guid id, [FromBody] AddNewsArticleTagRequest request)
        {
            _logger.LogInformation("Adding tag '{Tag}' to news article with ID: {NewsId}", SanitizeForLog(request.Tag), id);

            var command = new AddNewsArticleTagCommand(id, request.Tag);
            Result<bool> result = await _mediator.Send(command);

            return HandleResult(result, "Tag added to news article successfully", "Failed to add tag to news article");
        }

        /// <summary>
        /// Remove a tag from a news article
        /// </summary>
        /// <param name="id">The news article ID</param>
        /// <param name="request">The tag to remove</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id:guid}/tags")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveNewsTag(Guid id, [FromBody] RemoveNewsArticleTagRequest request)
        {
            _logger.LogInformation("Removing tag '{Tag}' from news article with ID: {NewsId}", SanitizeForLog(request.Tag), id);

            var command = new RemoveNewsArticleTagCommand(id, request.Tag);
            Result<bool> result = await _mediator.Send(command);

            return HandleResult(result, "Tag removed from news article successfully", "Failed to remove tag from news article");
        }

        /// <summary>
        /// Search news articles by search term
        /// </summary>
        /// <param name="request">The search parameters</param>
        /// <returns>List of matching news articles</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<List<NewsArticleListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<NewsArticleListDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<List<NewsArticleListDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<NewsArticleListDto>>>> SearchNews([FromQuery] SearchNewsArticlesRequest request)
        {
            _logger.LogInformation("Searching news articles with term: {SearchTerm}", SanitizeForLog(request.SearchTerm));

            var query = new SearchNewsArticlesQuery(request.SearchTerm);
            Result<IEnumerable<NewsArticleListDto>> result = await _mediator.Send(query);

            return HandleListResult(result, "News articles found successfully", "Failed to search news articles");
        }

        /// <summary>
        /// Get recent news articles
        /// </summary>
        /// <param name="request">The recent news parameters</param>
        /// <returns>List of recent news articles</returns>
        [HttpGet("recent")]
        [ProducesResponseType(typeof(ApiResponse<List<NewsArticleListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<NewsArticleListDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<NewsArticleListDto>>>> GetRecentNews([FromQuery] GetRecentNewsArticlesRequest request)
        {
            _logger.LogInformation("Getting recent news articles - Count: {Count}", request.Count);

            var query = new GetRecentNewsArticlesQuery(request.Count, request.IncludeArchived);
            Result<IEnumerable<NewsArticleListDto>> result = await _mediator.Send(query);

            return HandleListResult(result, "Recent news articles retrieved successfully", "Failed to retrieve recent news articles");
        }

        /// <summary>
        /// Get news articles by category
        /// </summary>
        /// <param name="category">The category to filter by</param>
        /// <returns>List of news articles in the specified category</returns>
        [HttpGet("category/{category}")]
        [ProducesResponseType(typeof(ApiResponse<List<NewsArticleListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<NewsArticleListDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<NewsArticleListDto>>>> GetNewsByCategory(string category)
        {
            _logger.LogInformation("Getting news articles by category: {Category}", SanitizeForLog(category));

            var query = new GetNewsArticlesByCategoryQuery(category);
            Result<IEnumerable<NewsArticleListDto>> result = await _mediator.Send(query);

            return HandleListResult(result, $"News articles in category '{category}' retrieved successfully", "Failed to retrieve news articles by category");
        }

        /// <summary>
        /// Get news articles by tag
        /// </summary>
        /// <param name="tag">The tag to filter by</param>
        /// <returns>List of news articles with the specified tag</returns>
        [HttpGet("tag/{tag}")]
        [ProducesResponseType(typeof(ApiResponse<List<NewsArticleListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<NewsArticleListDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<NewsArticleListDto>>>> GetNewsByTag(string tag)
        {
            _logger.LogInformation("Getting news articles by tag: {Tag}", SanitizeForLog(tag));

            var query = new GetNewsArticlesByTagQuery(tag);
            Result<IEnumerable<NewsArticleListDto>> result = await _mediator.Send(query);

            return HandleListResult(result, $"News articles with tag '{tag}' retrieved successfully", "Failed to retrieve news articles by tag");
        }

        /// <summary>
        /// Get news articles by author
        /// </summary>
        /// <param name="author">The author to filter by</param>
        /// <returns>List of news articles by the specified author</returns>
        [HttpGet("author/{author}")]
        [ProducesResponseType(typeof(ApiResponse<List<NewsArticleListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<NewsArticleListDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<NewsArticleListDto>>>> GetNewsByAuthor(string author)
        {
            _logger.LogInformation("Getting news articles by author: {Author}", SanitizeForLog(author));

            var query = new GetNewsArticlesByAuthorQuery(author);
            Result<IEnumerable<NewsArticleListDto>> result = await _mediator.Send(query);

            return HandleListResult(result, $"News articles by author '{author}' retrieved successfully", "Failed to retrieve news articles by author");
        }

        /// <summary>
        /// Get all available news categories
        /// </summary>
        /// <returns>List of available news categories</returns>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(ApiResponse<List<NewsArticleCategoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<NewsArticleCategoryDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<NewsArticleCategoryDto>>>> GetNewsCategories()
        {
            _logger.LogInformation("Getting all news categories");

            var query = new GetNewsArticleCategoriesQuery();
            Result<IEnumerable<NewsArticleCategoryDto>> result = await _mediator.Send(query);

            return HandleListResult(result, "News categories retrieved successfully", "Failed to retrieve news categories");
        }

        /// <summary>
        /// Get all used tags in news articles
        /// </summary>
        /// <returns>List of all used tags</returns>
        [HttpGet("tags")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetNewsTags()
        {
            _logger.LogInformation("Getting all news tags");

            var query = new GetNewsArticleTagsQuery();
            Result<IEnumerable<string>> result = await _mediator.Send(query);

            return HandleListResult(result, "News tags retrieved successfully", "Failed to retrieve news tags");
        }

        /// <summary>
        /// Upload an image and get its URL
        /// </summary>
        /// <param name="file">The image file to upload</param>
        /// <returns>The URL of the uploaded image</returns>
        [HttpPost("upload-image")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<string>>> UploadImage([FromForm] IFormFile file)
        {
            _logger.LogInformation("Uploading image: {FileName}", SanitizeForLog(file?.FileName));

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Image upload failed: No file provided");
                return BadRequest(ApiResponse<string>.ErrorResponse("No file provided"));
            }

            // Validate file type
            string[] allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
            {
                _logger.LogWarning("Image upload failed: Invalid file type {ContentType}", SanitizeForLog(file.ContentType));
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

                if (result.IsSuccess && result.Data is not null)
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
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<string>>> DeleteImage([FromQuery] string url)
        {
            _logger.LogInformation("Deleting image: {url}", SanitizeForLog(url));

            if (url == null)
            {
                _logger.LogWarning("Image deletion failed: No url provided");
                return BadRequest(ApiResponse<string>.ErrorResponse("No url provided"));
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? imageUri))
            {
                _logger.LogError("Failed to parse image deletion URL");
                return BadRequest(ApiResponse<string>.ErrorResponse("Invalid URL format"));
            }

            _logger.LogInformation("Successfully parsed image deletion URL");
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

        /// <summary>
        /// Gets the newest news as a main news
        /// </summary>
        /// <returns></returns>
        [HttpGet("main-news")]
        [ProducesResponseType(typeof(ApiResponse<NewsArticleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<NewsArticleDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<NewsArticleDto>>> GetMainNews()
        {
            GetMainNewsQuery query = new GetMainNewsQuery();
            Result<NewsArticleDto> result = await _mediator.Send(query);

            return HandleResult(result, "Main news retrieved successfully", "No main news found.");
        }

        /// <summary>
        /// Deletes news by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = AuthRoles.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<NewsArticleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteNews(Guid id)
        {
            _logger.LogInformation("Deleting news article with ID: {NewsId}", id);

            var command = new DeleteNewsArticleCommand(id);
            Result<bool> result = await _mediator.Send(command);

            return HandleResult(result, "News article deleted successfully", "Failed to delete news article");
        }
    }
}
