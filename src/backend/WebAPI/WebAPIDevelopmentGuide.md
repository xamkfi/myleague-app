# WebAPI Development Guide - League Management System

## Overview

This guide provides comprehensive instructions for developing controllers, middleware, and API features in the WebAPI layer. The guide follows REST API conventions, clean architecture principles, and ASP.NET Core best practices.

## 🎯 Before You Start

### Prerequisites
- Understanding of REST API principles and HTTP protocol
- Familiarity with ASP.NET Core and MVC patterns
- Knowledge of MediatR and CQRS patterns
- Experience with dependency injection
- Understanding of OpenAPI/Swagger documentation

### Key Principles to Follow
1. **REST API Conventions** - Follow standard HTTP methods and status codes
2. **Single Responsibility** - Each controller manages one resource type
3. **Consistent Response Format** - Use ApiResponse wrapper for all endpoints
4. **Comprehensive Validation** - Validate all inputs at the API boundary
5. **Proper Error Handling** - Return appropriate HTTP status codes and error messages

## 🚀 Development Process

### Step 1: API Design & Planning

#### 1.1 Resource Analysis
- [ ] Identify the resource being exposed (Club, Team, Player, etc.)
- [ ] Define CRUD operations needed
- [ ] Plan URL structure following REST conventions
- [ ] Design request/response DTOs
- [ ] Define validation rules and error scenarios

#### 1.2 HTTP Method Mapping
```
GET    /api/resource        - List all resources (with filtering/pagination)
GET    /api/resource/{id}   - Get specific resource by ID
POST   /api/resource        - Create new resource
PUT    /api/resource/{id}   - Update existing resource (full replacement)
PATCH  /api/resource/{id}   - Partial update of resource
DELETE /api/resource/{id}   - Delete resource
```

### Step 2: Controller Implementation

#### 2.1 Basic Controller Structure
```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Commands.Floorball;
using Application.Queries.Floorball;
using Application.DTOs.Floorball;
using Application.Common;
using WebAPI.Models.Floorball;

namespace WebAPI.Controllers.Floorball;

/// <summary>
/// Controller for managing floorball teams
/// </summary>
[ApiController]
[Route("api/floorball/[controller]")]
[Produces("application/json")]
public class TeamsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TeamsController> _logger;

    /// <summary>
    /// Initializes a new instance of the TeamsController class
    /// </summary>
    /// <param name="mediator">The mediator for handling commands and queries</param>
    /// <param name="logger">The logger for this controller</param>
    public TeamsController(IMediator mediator, ILogger<TeamsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // Controller actions go here...
}
```

#### 2.2 GET Operations (Queries)

**Get All Resources with Filtering:**
```csharp
/// <summary>
/// Get all floorball teams with optional filtering
/// </summary>
/// <param name="clubId">Optional club ID to filter by</param>
/// <param name="division">Optional division to filter by</param>
/// <param name="searchTerm">Optional search term for team name</param>
/// <param name="page">Page number (default: 1)</param>
/// <param name="pageSize">Page size (default: 50)</param>
/// <returns>Paginated list of teams</returns>
[HttpGet]
[ProducesResponseType(typeof(ApiResponse<PagedResult<FloorballTeamDto>>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<ApiResponse<PagedResult<FloorballTeamDto>>>> GetTeams(
    [FromQuery] Guid? clubId = null,
    [FromQuery] string? division = null,
    [FromQuery] string? searchTerm = null,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50)
{
    _logger.LogInformation("Getting floorball teams with filters: ClubId={ClubId}, Division={Division}, SearchTerm={SearchTerm}, Page={Page}, PageSize={PageSize}", 
        clubId, division, searchTerm, page, pageSize);

    GetFloorballTeamsQuery query = new GetFloorballTeamsQuery(
        clubId, division, searchTerm, page, pageSize);
    
    Result<PagedResult<FloorballTeamDto>> result = await _mediator.Send(query);

    if (result.IsSuccess && result.Value != null)
    {
        return Ok(ApiResponse<PagedResult<FloorballTeamDto>>.SuccessResponse(
            result.Value, "Teams retrieved successfully"));
    }

    string errorMessage = result.Error ?? "Failed to retrieve teams";
    return BadRequest(ApiResponse<PagedResult<FloorballTeamDto>>.ErrorResponse(errorMessage));
}
```

**Get Single Resource:**
```csharp
/// <summary>
/// Get a floorball team by ID
/// </summary>
/// <param name="id">Team ID</param>
/// <returns>Team details</returns>
[HttpGet("{id:guid}")]
[ProducesResponseType(typeof(ApiResponse<FloorballTeamDto>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<ApiResponse<FloorballTeamDto>>> GetTeamById(Guid id)
{
    _logger.LogInformation("Getting floorball team with ID: {TeamId}", id);
    
    GetFloorballTeamByIdQuery query = new GetFloorballTeamByIdQuery(id);
    Result<FloorballTeamDto> result = await _mediator.Send(query);

    if (result.IsSuccess && result.Value != null)
    {
        return Ok(ApiResponse<FloorballTeamDto>.SuccessResponse(
            result.Value, "Team retrieved successfully"));
    }

    string errorMessage = result.Error ?? "Team not found";
    if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
    {
        return NotFound(ApiResponse<FloorballTeamDto>.ErrorResponse(errorMessage));
    }

    return StatusCode(500, ApiResponse<FloorballTeamDto>.ErrorResponse(errorMessage));
}
```

#### 2.3 POST Operations (Create)

```csharp
/// <summary>
/// Create a new floorball team
/// </summary>
/// <param name="request">Team creation request</param>
/// <returns>Created team details</returns>
[HttpPost]
[ProducesResponseType(typeof(ApiResponse<FloorballTeamDto>), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<ApiResponse<FloorballTeamDto>>> CreateTeam([FromBody] CreateFloorballTeamRequest request)
{
    _logger.LogInformation("Creating new floorball team: {TeamName}", request.Name);

    CreateFloorballTeamCommand command = new CreateFloorballTeamCommand(
        request.Name,
        request.Description,
        request.ClubId,
        request.Division);

    Result<FloorballTeamDto> result = await _mediator.Send(command);

    if (result.IsSuccess && result.Value != null)
    {
        return CreatedAtAction(
            nameof(GetTeamById),
            new { id = result.Value.Id },
            ApiResponse<FloorballTeamDto>.SuccessResponse(result.Value, "Team created successfully"));
    }

    string errorMessage = result.Error ?? "Failed to create team";
    return BadRequest(ApiResponse<FloorballTeamDto>.ErrorResponse(errorMessage));
}
```

#### 2.4 PUT Operations (Full Update)

```csharp
/// <summary>
/// Update an existing floorball team
/// </summary>
/// <param name="id">Team ID</param>
/// <param name="request">Team update request</param>
/// <returns>Updated team details</returns>
[HttpPut("{id:guid}")]
[ProducesResponseType(typeof(ApiResponse<FloorballTeamDto>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<ApiResponse<FloorballTeamDto>>> UpdateTeam(Guid id, [FromBody] UpdateFloorballTeamRequest request)
{
    _logger.LogInformation("Updating floorball team with ID: {TeamId}", id);

    UpdateFloorballTeamCommand command = new UpdateFloorballTeamCommand(
        id,
        request.Name,
        request.Description,
        request.Division);

    Result<FloorballTeamDto> result = await _mediator.Send(command);

    if (result.IsSuccess && result.Value != null)
    {
        return Ok(ApiResponse<FloorballTeamDto>.SuccessResponse(
            result.Value, "Team updated successfully"));
    }

    string errorMessage = result.Error ?? "Failed to update team";
    
    if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
    {
        return NotFound(ApiResponse<FloorballTeamDto>.ErrorResponse(errorMessage));
    }

    return BadRequest(ApiResponse<FloorballTeamDto>.ErrorResponse(errorMessage));
}
```

#### 2.5 DELETE Operations

```csharp
/// <summary>
/// Delete a floorball team
/// </summary>
/// <param name="id">Team ID</param>
/// <returns>Success confirmation</returns>
[HttpDelete("{id:guid}")]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<ApiResponse>> DeleteTeam(Guid id)
{
    _logger.LogInformation("Deleting floorball team with ID: {TeamId}", id);

    DeleteFloorballTeamCommand command = new DeleteFloorballTeamCommand(id);
    Result result = await _mediator.Send(command);

    if (result.IsSuccess)
    {
        return Ok(ApiResponse.SuccessResponse("Team deleted successfully"));
    }

    string errorMessage = result.Error ?? "Failed to delete team";

    if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
    {
        return NotFound(ApiResponse.ErrorResponse(errorMessage));
    }

    return StatusCode(500, ApiResponse.ErrorResponse(errorMessage));
}
```

### Step 3: Request/Response Models

#### 3.1 Request Models
```csharp
namespace WebAPI.Models.Floorball;

/// <summary>
/// Request model for creating a new floorball team
/// </summary>
public record CreateFloorballTeamRequest
{
    /// <summary>
    /// Team name
    /// </summary>
    /// <example>Manchester United Floorball</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Team description
    /// </summary>
    /// <example>Professional floorball team based in Manchester</example>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Club ID this team belongs to
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public Guid ClubId { get; init; }

    /// <summary>
    /// Division the team plays in
    /// </summary>
    /// <example>FirstDivision</example>
    public string Division { get; init; } = string.Empty;
}

/// <summary>
/// Request model for updating an existing floorball team
/// </summary>
public record UpdateFloorballTeamRequest
{
    /// <summary>
    /// Team name
    /// </summary>
    /// <example>Manchester United Floorball</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Team description
    /// </summary>
    /// <example>Professional floorball team based in Manchester</example>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Division the team plays in
    /// </summary>
    /// <example>FirstDivision</example>
    public string Division { get; init; } = string.Empty;
}
```

### Step 4: Middleware Development

#### 4.1 Custom Middleware Structure
```csharp
namespace WebAPI.Middlewares;

/// <summary>
/// Middleware for request/response logging and performance monitoring
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate correlation ID
        string correlationId = Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;

        // Log request
        _logger.LogInformation("Request started: {Method} {Path} - CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            // Log response
            _logger.LogInformation("Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms - CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);
        }
    }
}
```

#### 4.2 Rate Limiting Middleware
```csharp
namespace WebAPI.Middlewares;

/// <summary>
/// Simple rate limiting middleware
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly Dictionary<string, List<DateTime>> _requestTimes = new();
    private readonly int _maxRequests = 100;
    private readonly TimeSpan _timeWindow = TimeSpan.FromMinutes(1);

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string clientId = GetClientId(context);
        
        if (IsRateLimitExceeded(clientId))
        {
            _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientId);
            
            context.Response.StatusCode = 429; // Too Many Requests
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        await _next(context);
    }

    private string GetClientId(HttpContext context)
    {
        // Use IP address as client identifier
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private bool IsRateLimitExceeded(string clientId)
    {
        DateTime now = DateTime.UtcNow;
        
        if (!_requestTimes.ContainsKey(clientId))
        {
            _requestTimes[clientId] = new List<DateTime>();
        }

        List<DateTime> clientRequests = _requestTimes[clientId];
        
        // Remove old requests outside the time window
        clientRequests.RemoveAll(time => now - time > _timeWindow);
        
        // Check if limit is exceeded
        if (clientRequests.Count >= _maxRequests)
        {
            return true;
        }

        // Add current request
        clientRequests.Add(now);
        return false;
    }
}
```

### Step 5: Service Extensions

#### 5.1 API Configuration Extensions
```csharp
namespace WebAPI.DependencyInjections;

/// <summary>
/// API versioning configuration
/// </summary>
public static class ApiVersioningExtensions
{
    public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Version"),
                new MediaTypeApiVersionReader("ver"));
        });

        services.AddVersionedApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}

/// <summary>
/// Authentication configuration
/// </summary>
public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        JwtSettings jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
            };
        });

        return services;
    }
}
```

### Step 6: Testing

#### 6.1 Controller Unit Tests
```csharp
public class FloorballTeamsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<TeamsController>> _loggerMock;
    private readonly TeamsController _controller;

    public FloorballTeamsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<TeamsController>>();
        _controller = new TeamsController(_mediatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetTeamById_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        Guid teamId = Guid.NewGuid();
        FloorballTeamDto teamDto = new FloorballTeamDto(teamId, "Test Team", "Description", Guid.NewGuid(), "Test Club", "FirstDivision", 0, DateTime.UtcNow, null);
        Result<FloorballTeamDto> result = Result<FloorballTeamDto>.Success(teamDto);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFloorballTeamByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        ActionResult<ApiResponse<FloorballTeamDto>> response = await _controller.GetTeamById(teamId);

        // Assert
        OkObjectResult okResult = response.Result.Should().BeOfType<OkObjectResult>().Subject;
        ApiResponse<FloorballTeamDto> apiResponse = okResult.Value.Should().BeOfType<ApiResponse<FloorballTeamDto>>().Subject;
        
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().Be(teamDto);
        apiResponse.Message.Should().Be("Team retrieved successfully");
    }

    [Fact]
    public async Task GetTeamById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        Guid teamId = Guid.NewGuid();
        Result<FloorballTeamDto> result = Result<FloorballTeamDto>.Failure("Team not found");

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFloorballTeamByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        ActionResult<ApiResponse<FloorballTeamDto>> response = await _controller.GetTeamById(teamId);

        // Assert
        NotFoundObjectResult notFoundResult = response.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        ApiResponse<FloorballTeamDto> apiResponse = notFoundResult.Value.Should().BeOfType<ApiResponse<FloorballTeamDto>>().Subject;
        
        apiResponse.Success.Should().BeFalse();
        apiResponse.Errors.Should().Contain("Team not found");
    }

    [Fact]
    public async Task CreateTeam_WithValidRequest_ShouldReturnCreatedResult()
    {
        // Arrange
        CreateFloorballTeamRequest request = new CreateFloorballTeamRequest
        {
            Name = "Test Team",
            Description = "Test Description",
            ClubId = Guid.NewGuid(),
            Division = "FirstDivision"
        };

        FloorballTeamDto createdTeam = new FloorballTeamDto(Guid.NewGuid(), request.Name, request.Description, request.ClubId, "Test Club", request.Division, 0, DateTime.UtcNow, null);
        Result<FloorballTeamDto> result = Result<FloorballTeamDto>.Success(createdTeam);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateFloorballTeamCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        ActionResult<ApiResponse<FloorballTeamDto>> response = await _controller.CreateTeam(request);

        // Assert
        CreatedAtActionResult createdResult = response.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        ApiResponse<FloorballTeamDto> apiResponse = createdResult.Value.Should().BeOfType<ApiResponse<FloorballTeamDto>>().Subject;
        
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().Be(createdTeam);
        apiResponse.Message.Should().Be("Team created successfully");
        
        createdResult.ActionName.Should().Be(nameof(TeamsController.GetTeamById));
        createdResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(createdTeam.Id);
    }
}
```

#### 6.2 Integration Tests
```csharp
public class FloorballTeamsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public FloorballTeamsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetTeams_ShouldReturnOkWithTeamsList()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/floorball/teams");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        string content = await response.Content.ReadAsStringAsync();
        ApiResponse<PagedResult<FloorballTeamDto>>? apiResponse = JsonSerializer.Deserialize<ApiResponse<PagedResult<FloorballTeamDto>>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTeam_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        CreateFloorballTeamRequest request = new CreateFloorballTeamRequest
        {
            Name = "Integration Test Team",
            Description = "Test team for integration testing",
            ClubId = Guid.NewGuid(), // This should be a valid club ID in your test database
            Division = "FirstDivision"
        };

        string jsonRequest = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/floorball/teams", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        string responseContent = await response.Content.ReadAsStringAsync();
        ApiResponse<FloorballTeamDto>? apiResponse = JsonSerializer.Deserialize<ApiResponse<FloorballTeamDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Name.Should().Be(request.Name);
    }
}
```

#### 6.3 Middleware Tests
```csharp
public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithUnhandledException_ShouldReturnInternalServerError()
    {
        // Arrange
        Mock<ILogger<ExceptionHandlingMiddleware>> loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        RequestDelegate next = _ => throw new Exception("Test exception");
        
        ExceptionHandlingMiddleware middleware = new ExceptionHandlingMiddleware(next, loggerMock.Object);
        
        DefaultHttpContext context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(500);
        context.Response.ContentType.Should().Be("application/json");
        
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        string responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        
        ApiResponse? apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Errors.Should().Contain("An internal server error occurred");
    }
}
```

## 📋 WebAPI Development Checklist

### Design Phase
- [ ] REST API resource and operations identified
- [ ] HTTP methods and status codes planned
- [ ] Request/response models designed
- [ ] URL structure follows REST conventions
- [ ] Error handling scenarios defined

### Implementation Phase
- [ ] Controller class created with proper routing
- [ ] All CRUD operations implemented
- [ ] Request/response models created
- [ ] Proper HTTP status codes returned
- [ ] Comprehensive error handling implemented
- [ ] Logging statements added

### Documentation Phase
- [ ] XML documentation comments added
- [ ] OpenAPI attributes configured
- [ ] Request/response examples provided
- [ ] Error codes documented

### Testing Phase
- [ ] Unit tests for all controller actions
- [ ] Integration tests for key workflows
- [ ] Error scenario tests implemented
- [ ] Middleware tests created

### Quality Phase
- [ ] Code review completed
- [ ] API conventions followed
- [ ] Performance considerations addressed
- [ ] Security best practices implemented

## 🔧 Common Patterns & Examples

### Adding New Controllers
1. Create controller class in appropriate folder (Common/Floorball/Hockey)
2. Implement standard CRUD operations
3. Add request/response models
4. Configure routing and documentation
5. Write comprehensive tests

### Implementing Pagination for List Endpoints
When creating GET endpoints that return collections, always implement pagination:

1. **Request Model**: Include `Page` and `PageSize` parameters in your request model
   ```csharp
   public record GetResourcesRequest
   {
       [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
       public int Page { get; init; } = 1;
       
       [Range(0, 100, ErrorMessage = "Page size must be between 0 and 100")]
       public int PageSize { get; init; } = 0; // 0 means use default
       
       // Add your filter properties here
   }
   ```

2. **Controller Action**: Use `PaginatedApiResponse<T>` for the response
   ```csharp
   [HttpGet]
   public async Task<ActionResult<PaginatedApiResponse<ResourceDto>>> GetResources([FromQuery] GetResourcesRequest request)
   {
       var query = new GetResourcesQuery(request.Page, request.PageSize, /* filters */);
       var result = await _mediator.Send(query);
       
       return result.IsSuccess 
           ? Ok(PaginatedApiResponse<ResourceDto>.SuccessResponse(result.Data!, "Resources retrieved successfully"))
           : BadRequest(PaginatedApiResponse<ResourceDto>.ErrorResponse(result.Error!));
   }
   ```

3. **Query Handler**: Extend `BasePagedQueryHandler<TQuery, TDto>` and use `PagedResult<T>`
   ```csharp
   public class GetResourcesHandler : BasePagedQueryHandler<GetResourcesQuery, ResourceDto>
   {
       public async Task<Result<PagedResult<ResourceDto>>> Handle(GetResourcesQuery request, CancellationToken cancellationToken)
       {
           // Validate pagination parameters
           var validationResult = ValidatePaginationParameters(request.Page, request.PageSize, "ResourceKey");
           if (validationResult.IsFailure) return Result<PagedResult<ResourceDto>>.Failure(validationResult.Error!);
           
           // Get data and create paged result
           var pagedResult = CreatePagedResult(items, totalCount, request.Page, actualPageSize);
           return Result<PagedResult<ResourceDto>>.Success(pagedResult);
       }
   }
   ```

4. **Configuration**: Ensure your resource is configured in `appsettings.json` under `PaginationSettings`

This ensures consistent pagination behavior across all list endpoints with proper validation, error handling, and rich metadata for clients.

### Custom Middleware Development
1. Implement middleware class with InvokeAsync method
2. Register middleware in Program.cs
3. Consider middleware order and dependencies
4. Add proper error handling and logging
5. Write unit tests for middleware logic

### Error Handling Patterns
1. Use Result pattern from Application layer
2. Map business errors to appropriate HTTP status codes
3. Return consistent ApiResponse format
4. Log errors with appropriate level
5. Don't expose sensitive information in error messages

## ⚠️ Common Pitfalls to Avoid

1. **Breaking REST Conventions** - Follow standard HTTP methods and status codes
2. **Inconsistent Response Format** - Always use ApiResponse wrapper
3. **Missing Error Handling** - Handle all possible error scenarios
4. **Poor Logging** - Log important operations and errors with context
5. **Missing Validation** - Validate all inputs at API boundary
6. **Exposing Internal Details** - Don't leak implementation details in responses
7. **Not Using Async** - Use async/await for all I/O operations
8. **Missing Documentation** - Document all endpoints with XML comments

## 📊 Performance Considerations

### Response Time Optimization
- Use async/await throughout the request pipeline
- Implement response caching for read operations
- Use pagination for large datasets
- Optimize database queries through proper repository design

### Memory Management
- Dispose resources properly in middleware
- Use streaming for large file uploads/downloads
- Implement proper model binding for large requests
- Monitor memory usage in long-running operations

### Scalability
- Design stateless controllers
- Use distributed caching when appropriate
- Implement rate limiting for API protection
- Consider response compression for large payloads 
