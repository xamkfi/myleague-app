# Application Development Guide - League Management System

## Overview

This guide provides comprehensive instructions for developing new features in the Application layer using CQRS patterns, MediatR, FluentValidation, and following clean architecture principles.

## 🎯 Before You Start

### Prerequisites
- Understanding of CQRS (Command Query Responsibility Segregation) pattern
- Familiarity with MediatR mediator pattern
- Knowledge of FluentValidation framework
- Understanding of the Result pattern
- Experience with dependency injection patterns

### Key Principles to Follow
1. **CQRS Separation** - Strict separation between commands and queries
2. **Single Responsibility** - Each handler has one specific purpose
3. **Validation First** - All input must be validated before processing
4. **Result Pattern** - Consistent success/failure response structure
5. **Async Operations** - All operations should be asynchronous

## 🚀 Development Process

### Step 1: Feature Analysis & Design

#### 1.1 Understand the Business Requirement
- [ ] Identify if this is a command (write) or query (read) operation
- [ ] Determine required input parameters and validation rules
- [ ] Define expected output format and DTOs
- [ ] Understand business rules and domain constraints
- [ ] Plan error handling scenarios

#### 1.2 Application Design
- [ ] Design command/query structure
- [ ] Plan DTO mappings
- [ ] Design validation rules
- [ ] Plan handler implementation
- [ ] Consider cross-cutting concerns (caching, logging)

#### 1.3 Implementation Planning
- [ ] Plan service dependencies
- [ ] Design unit tests
- [ ] Consider performance implications
- [ ] Plan integration points

### Step 2: Implementation

#### 2.1 Commands (Write Operations)

**For Creating New Entities:**
```csharp
// Command Definition
namespace Application.Commands.Floorball
{
    public record CreateFloorballTeamCommand(
        string Name,
        string Description,
        Guid ClubId,
        string Division
    ) : IRequest<Result<FloorballTeamDto>>;
}

// Command Validator
namespace Application.Validators.Commands.Floorball
{
    public class CreateFloorballTeamCommandValidator : AbstractValidator<CreateFloorballTeamCommand>
    {
        public CreateFloorballTeamCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Team name is required")
                .MaximumLength(100)
                .WithMessage("Team name must not exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.ClubId)
                .NotEmpty()
                .WithMessage("Club ID is required");

            RuleFor(x => x.Division)
                .NotEmpty()
                .WithMessage("Division is required")
                .Must(BeValidDivision)
                .WithMessage("Invalid division specified");
        }

        private bool BeValidDivision(string division)
        {
            return Enum.TryParse<FloorballDivision>(division, true, out _);
        }
    }
}

// Command Handler
namespace Application.Handlers.Floorball
{
    public class CreateFloorballTeamCommandHandler : IRequestHandler<CreateFloorballTeamCommand, Result<FloorballTeamDto>>
    {
        private readonly IFloorballTeamRepository _teamRepository;
        private readonly IClubRepository _clubRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateFloorballTeamCommandHandler> _logger;

        public CreateFloorballTeamCommandHandler(
            IFloorballTeamRepository teamRepository,
            IClubRepository clubRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateFloorballTeamCommandHandler> logger)
        {
            _teamRepository = teamRepository;
            _clubRepository = clubRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<FloorballTeamDto>> Handle(CreateFloorballTeamCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating floorball team {TeamName} for club {ClubId}", 
                    request.Name, request.ClubId);

                // 1. Verify club exists
                Club? club = await _clubRepository.GetByIdAsync(new ClubId(request.ClubId), cancellationToken);
                if (club == null)
                {
                    return Result<FloorballTeamDto>.Failure("Club not found");
                }

                // 2. Check for duplicate team name within club
                FloorballTeam? existingTeam = await _teamRepository.GetByNameAndClubAsync(request.Name, new ClubId(request.ClubId), cancellationToken);
                if (existingTeam != null)
                {
                    return Result<FloorballTeamDto>.Failure("A team with this name already exists for this club");
                }

                // 3. Create domain entity
                FloorballTeamId teamId = new FloorballTeamId(Guid.NewGuid());
                FloorballDivision division = Enum.Parse<FloorballDivision>(request.Division, true);
                
                FloorballTeam team = new FloorballTeam(
                    teamId,
                    request.Name,
                    request.Description,
                    new ClubId(request.ClubId),
                    division);

                // 4. Persist entity
                await _teamRepository.SaveAsync(team, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 5. Map to DTO and return
                FloorballTeamDto teamDto = _mapper.Map<FloorballTeamDto>(team);
                
                _logger.LogInformation("Successfully created floorball team {TeamId}", teamId.Value);
                return Result<FloorballTeamDto>.Success(teamDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating floorball team {TeamName}", request.Name);
                return Result<FloorballTeamDto>.Failure("An error occurred while creating the team");
            }
        }
    }
}
```

#### 2.2 Queries (Read Operations)

```csharp
// Query Definition
namespace Application.Queries.Floorball
{
    public record GetFloorballTeamByIdQuery(
        Guid TeamId
    ) : IRequest<Result<FloorballTeamDto>>;

    public record GetFloorballTeamsQuery(
        Guid? ClubId = null,
        string? Division = null,
        string? SearchTerm = null,
        int Page = 1,
        int PageSize = 50
    ) : IRequest<Result<PagedResult<FloorballTeamDto>>>;
}

// Query Validator
namespace Application.Validators.Queries.Floorball
{
    public class GetFloorballTeamsQueryValidator : AbstractValidator<GetFloorballTeamsQuery>
    {
        public GetFloorballTeamsQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.Division)
                .Must(BeValidDivisionOrNull)
                .WithMessage("Invalid division specified")
                .When(x => !string.IsNullOrEmpty(x.Division));
        }

        private bool BeValidDivisionOrNull(string? division)
        {
            return string.IsNullOrEmpty(division) || Enum.TryParse<FloorballDivision>(division, true, out _);
        }
    }
}

// Query Handler
namespace Application.Handlers.Floorball
{
    public class GetFloorballTeamsQueryHandler : IRequestHandler<GetFloorballTeamsQuery, Result<PagedResult<FloorballTeamDto>>>
    {
        private readonly IFloorballTeamRepository _teamRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetFloorballTeamsQueryHandler> _logger;
        private readonly IMemoryCache _cache;

        public GetFloorballTeamsQueryHandler(
            IFloorballTeamRepository teamRepository,
            IMapper mapper,
            ILogger<GetFloorballTeamsQueryHandler> logger,
            IMemoryCache cache)
        {
            _teamRepository = teamRepository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<Result<PagedResult<FloorballTeamDto>>> Handle(GetFloorballTeamsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving floorball teams with filters: ClubId={ClubId}, Division={Division}, SearchTerm={SearchTerm}", 
                    request.ClubId, request.Division, request.SearchTerm);

                // Check cache first
                string cacheKey = $"floorball_teams_{request.ClubId}_{request.Division}_{request.SearchTerm}_{request.Page}_{request.PageSize}";
                if (_cache.TryGetValue(cacheKey, out PagedResult<FloorballTeamDto>? cachedResult))
                {
                    _logger.LogInformation("Returning cached result for floorball teams query");
                    return Result<PagedResult<FloorballTeamDto>>.Success(cachedResult!);
                }

                // Parse division if provided
                FloorballDivision? division = null;
                if (!string.IsNullOrEmpty(request.Division))
                {
                    division = Enum.Parse<FloorballDivision>(request.Division, true);
                }

                // Retrieve from repository
                IEnumerable<FloorballTeam> teams = await _teamRepository.GetPagedAsync(
                    request.ClubId.HasValue ? new ClubId(request.ClubId.Value) : null,
                    division,
                    request.SearchTerm,
                    request.Page,
                    request.PageSize,
                    cancellationToken);

                int totalCount = await _teamRepository.CountAsync(
                    request.ClubId.HasValue ? new ClubId(request.ClubId.Value) : null,
                    division,
                    request.SearchTerm,
                    cancellationToken);

                // Map to DTOs
                IEnumerable<FloorballTeamDto> teamDtos = _mapper.Map<IEnumerable<FloorballTeamDto>>(teams);
                
                PagedResult<FloorballTeamDto> result = new PagedResult<FloorballTeamDto>(
                    teamDtos,
                    totalCount,
                    request.Page,
                    request.PageSize,
                    (int)Math.Ceiling((double)totalCount / request.PageSize));

                // Cache the result
                MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                };
                _cache.Set(cacheKey, result, cacheOptions);

                _logger.LogInformation("Successfully retrieved {Count} floorball teams", teamDtos.Count());
                return Result<PagedResult<FloorballTeamDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving floorball teams");
                return Result<PagedResult<FloorballTeamDto>>.Failure("An error occurred while retrieving teams");
            }
        }
    }
}
```

#### 2.3 Data Transfer Objects (DTOs)

```csharp
namespace Application.DTOs.Floorball
{
    public record FloorballTeamDto(
        Guid Id,
        string Name,
        string Description,
        Guid ClubId,
        string ClubName,
        string Division,
        int PlayerCount,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );

    public record FloorballTeamSummaryDto(
        Guid Id,
        string Name,
        string ClubName,
        string Division,
        int PlayerCount
    );

    public record CreateFloorballTeamRequest(
        string Name,
        string Description,
        Guid ClubId,
        string Division
    );

    public record UpdateFloorballTeamRequest(
        string Name,
        string Description,
        string Division
    );
}
```

#### 2.4 Object Mapping Configuration

```csharp
namespace Application.Mappings.Floorball
{
    public class FloorballTeamMappingProfile : Profile
    {
        public FloorballTeamMappingProfile()
        {
            CreateMap<FloorballTeam, FloorballTeamDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
                .ForMember(dest => dest.ClubId, opt => opt.MapFrom(src => src.ClubId.Value))
                .ForMember(dest => dest.ClubName, opt => opt.MapFrom(src => src.Club.Name))
                .ForMember(dest => dest.Division, opt => opt.MapFrom(src => src.Division.ToString()))
                .ForMember(dest => dest.PlayerCount, opt => opt.MapFrom(src => src.Players.Count));

            CreateMap<FloorballTeam, FloorballTeamSummaryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
                .ForMember(dest => dest.ClubName, opt => opt.MapFrom(src => src.Club.Name))
                .ForMember(dest => dest.Division, opt => opt.MapFrom(src => src.Division.ToString()))
                .ForMember(dest => dest.PlayerCount, opt => opt.MapFrom(src => src.Players.Count));

            CreateMap<CreateFloorballTeamRequest, CreateFloorballTeamCommand>();
            CreateMap<UpdateFloorballTeamRequest, UpdateFloorballTeamCommand>();
        }
    }
}
```

#### 2.5 Pipeline Behaviors

**Caching Behavior:**
```csharp
namespace Application.Behaviors
{
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

        public CachingBehavior(IMemoryCache cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Only cache queries, not commands
            if (!IsQuery(request))
            {
                return await next();
            }

            string cacheKey = GenerateCacheKey(request);
            
            if (_cache.TryGetValue(cacheKey, out TResponse? cachedResponse))
            {
                _logger.LogInformation("Cache hit for {RequestType}", typeof(TRequest).Name);
                return cachedResponse!;
            }

            _logger.LogInformation("Cache miss for {RequestType}", typeof(TRequest).Name);
            TResponse response = await next();

            MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            _cache.Set(cacheKey, response, cacheOptions);
            return response;
        }

        private static bool IsQuery(TRequest request)
        {
            return request.GetType().Name.EndsWith("Query");
        }

        private static string GenerateCacheKey(TRequest request)
        {
            string requestType = request.GetType().Name;
            string requestData = JsonSerializer.Serialize(request);
            return $"{requestType}_{requestData.GetHashCode()}";
        }
    }
}

// Logging Behavior
namespace Application.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            string requestName = typeof(TRequest).Name;
            Guid requestId = Guid.NewGuid();

            _logger.LogInformation("Starting request {RequestName} with ID {RequestId}", requestName, requestId);

            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                TResponse response = await next();
                
                stopwatch.Stop();
                _logger.LogInformation("Completed request {RequestName} with ID {RequestId} in {ElapsedMs}ms", 
                    requestName, requestId, stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Request {RequestName} with ID {RequestId} failed after {ElapsedMs}ms", 
                    requestName, requestId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
```

#### 2.6 Service Registration

```csharp
namespace Application.DependencyInjections
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Register MediatR
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
                
                // Register behaviors in order
                cfg.AddBehavior<LoggingBehavior<,>>();
                cfg.AddBehavior<ValidationBehavior<,>>();
                cfg.AddBehavior<CachingBehavior<,>>();
            });

            // Register FluentValidation
            services.AddValidatorsFromAssembly(assembly);

            // Register AutoMapper
            services.AddAutoMapper(assembly);

            // Register memory cache for caching behavior
            services.AddMemoryCache();

            return services;
        }
    }
}
```

### Step 3: Testing

#### 3.1 Command Handler Tests

```csharp
public class CreateFloorballTeamCommandHandlerTests
{
    private readonly Mock<IFloorballTeamRepository> _teamRepositoryMock;
    private readonly Mock<IClubRepository> _clubRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CreateFloorballTeamCommandHandler>> _loggerMock;
    private readonly CreateFloorballTeamCommandHandler _handler;

    public CreateFloorballTeamCommandHandlerTests()
    {
        _teamRepositoryMock = new Mock<IFloorballTeamRepository>();
        _clubRepositoryMock = new Mock<IClubRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CreateFloorballTeamCommandHandler>>();

        _handler = new CreateFloorballTeamCommandHandler(
            _teamRepositoryMock.Object,
            _clubRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        CreateFloorballTeamCommand command = new CreateFloorballTeamCommand("Test Team", "Description", clubId, "FirstDivision");
        Club club = new Club(new ClubId(clubId), "Test Club", "Club Description", new Address("Street", "City", "12345", "Country"));
        FloorballTeamDto teamDto = new FloorballTeamDto(Guid.NewGuid(), "Test Team", "Description", clubId, "Test Club", "FirstDivision", 0, DateTime.UtcNow, null);

        _clubRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ClubId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(club);

        _teamRepositoryMock
            .Setup(x => x.GetByNameAndClubAsync(It.IsAny<string>(), It.IsAny<ClubId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FloorballTeam?)null);

        _mapperMock
            .Setup(x => x.Map<FloorballTeamDto>(It.IsAny<FloorballTeam>()))
            .Returns(teamDto);

        // Act
        Result<FloorballTeamDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(teamDto);
        
        _teamRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<FloorballTeam>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentClub_ShouldReturnFailureResult()
    {
        // Arrange
        CreateFloorballTeamCommand command = new CreateFloorballTeamCommand("Test Team", "Description", Guid.NewGuid(), "FirstDivision");

        _clubRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ClubId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Club?)null);

        // Act
        Result<FloorballTeamDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Club not found");
        
        _teamRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<FloorballTeam>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithDuplicateTeamName_ShouldReturnFailureResult()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        CreateFloorballTeamCommand command = new CreateFloorballTeamCommand("Test Team", "Description", clubId, "FirstDivision");
        Club club = new Club(new ClubId(clubId), "Test Club", "Club Description", new Address("Street", "City", "12345", "Country"));
        FloorballTeam existingTeam = new FloorballTeam(new FloorballTeamId(Guid.NewGuid()), "Test Team", "Description", new ClubId(clubId), FloorballDivision.FirstDivision);

        _clubRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ClubId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(club);

        _teamRepositoryMock
            .Setup(x => x.GetByNameAndClubAsync(It.IsAny<string>(), It.IsAny<ClubId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTeam);

        // Act
        Result<FloorballTeamDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("A team with this name already exists for this club");
        
        _teamRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<FloorballTeam>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
```

#### 3.2 Query Handler Tests

```csharp
public class GetFloorballTeamsQueryHandlerTests
{
    private readonly Mock<IFloorballTeamRepository> _teamRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<GetFloorballTeamsQueryHandler>> _loggerMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly GetFloorballTeamsQueryHandler _handler;

    public GetFloorballTeamsQueryHandlerTests()
    {
        _teamRepositoryMock = new Mock<IFloorballTeamRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<GetFloorballTeamsQueryHandler>>();
        _cacheMock = new Mock<IMemoryCache>();

        _handler = new GetFloorballTeamsQueryHandler(
            _teamRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnPagedResult()
    {
        // Arrange
        GetFloorballTeamsQuery query = new GetFloorballTeamsQuery(Page: 1, PageSize: 10);
        List<FloorballTeam> teams = new List<FloorballTeam>
        {
            new(new FloorballTeamId(Guid.NewGuid()), "Team 1", "Description", new ClubId(Guid.NewGuid()), FloorballDivision.FirstDivision),
            new(new FloorballTeamId(Guid.NewGuid()), "Team 2", "Description", new ClubId(Guid.NewGuid()), FloorballDivision.SecondDivision)
        };
        IEnumerable<FloorballTeamDto> teamDtos = teams.Select(t => new FloorballTeamDto(t.Id.Value, t.Name, t.Description, t.ClubId.Value, "Club Name", t.Division.ToString(), 0, DateTime.UtcNow, null));

        _cacheMock
            .Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object?>.IsAny))
            .Returns(false);

        _teamRepositoryMock
            .Setup(x => x.GetPagedAsync(null, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teams);

        _teamRepositoryMock
            .Setup(x => x.CountAsync(null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        _mapperMock
            .Setup(x => x.Map<IEnumerable<FloorballTeamDto>>(It.IsAny<IEnumerable<FloorballTeam>>()))
            .Returns(teamDtos);

        // Act
        Result<PagedResult<FloorballTeamDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WithCachedResult_ShouldReturnCachedData()
    {
        // Arrange
        GetFloorballTeamsQuery query = new GetFloorballTeamsQuery(Page: 1, PageSize: 10);
        PagedResult<FloorballTeamDto> cachedResult = new PagedResult<FloorballTeamDto>(
            new List<FloorballTeamDto>(),
            0, 1, 10, 0);

        object? cachedValue = cachedResult;
        _cacheMock
            .Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(true);

        // Act
        Result<PagedResult<FloorballTeamDto>> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(cachedResult);
        
        _teamRepositoryMock.Verify(x => x.GetPagedAsync(It.IsAny<ClubId?>(), It.IsAny<FloorballDivision?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
```

#### 3.3 Validator Tests

```csharp
public class CreateFloorballTeamCommandValidatorTests
{
    private readonly CreateFloorballTeamCommandValidator _validator;

    public CreateFloorballTeamCommandValidatorTests()
    {
        _validator = new CreateFloorballTeamCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPassValidation()
    {
        // Arrange
        CreateFloorballTeamCommand command = new CreateFloorballTeamCommand("Test Team", "Description", Guid.NewGuid(), "FirstDivision");

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_WithInvalidName_ShouldFailValidation(string name)
    {
        // Arrange
        CreateFloorballTeamCommand command = new CreateFloorballTeamCommand(name, "Description", Guid.NewGuid(), "FirstDivision");

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFloorballTeamCommand.Name));
    }

    [Fact]
    public void Validate_WithTooLongName_ShouldFailValidation()
    {
        // Arrange
        string longName = new string('A', 101);
        CreateFloorballTeamCommand command = new CreateFloorballTeamCommand(longName, "Description", Guid.NewGuid(), "FirstDivision");

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFloorballTeamCommand.Name));
    }

    [Fact]
    public void Validate_WithInvalidDivision_ShouldFailValidation()
    {
        // Arrange
        CreateFloorballTeamCommand command = new CreateFloorballTeamCommand("Test Team", "Description", Guid.NewGuid(), "InvalidDivision");

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFloorballTeamCommand.Division));
    }
}
```

#### 3.4 Integration Tests

```csharp
public class FloorballTeamIntegrationTests : IClassFixture<ApplicationTestFixture>, IDisposable
{
    private readonly ApplicationTestFixture _fixture;
    private readonly IServiceScope _scope;
    private readonly IMediator _mediator;

    public FloorballTeamIntegrationTests(ApplicationTestFixture fixture)
    {
        _fixture = fixture;
        _scope = _fixture.ServiceProvider.CreateScope();
        _mediator = _scope.ServiceProvider.GetRequiredService<IMediator>();
    }

    public void Dispose()
    {
        _scope.Dispose();
    }

    [Fact]
    public async Task CreateAndRetrieveFloorballTeam_ShouldWorkEndToEnd()
    {
        // Arrange
        CreateClubCommand createClubCommand = new CreateClubCommand("Test Club", "Description", new Address("Street", "City", "12345", "Country"));
        Result<ClubDto> clubResult = await _mediator.Send(createClubCommand);
        clubResult.IsSuccess.Should().BeTrue();

        CreateFloorballTeamCommand createTeamCommand = new CreateFloorballTeamCommand("Test Team", "Team Description", clubResult.Value.Id, "FirstDivision");

        // Act - Create team
        Result<FloorballTeamDto> createResult = await _mediator.Send(createTeamCommand);

        // Assert - Create succeeded
        createResult.IsSuccess.Should().BeTrue();
        createResult.Value.Name.Should().Be("Test Team");

        // Act - Retrieve team
        GetFloorballTeamByIdQuery getQuery = new GetFloorballTeamByIdQuery(createResult.Value.Id);
        Result<FloorballTeamDto> getResult = await _mediator.Send(getQuery);

        // Assert - Retrieve succeeded
        getResult.IsSuccess.Should().BeTrue();
        getResult.Value.Should().BeEquivalentTo(createResult.Value);
    }

    [Fact]
    public async Task CreateFloorballTeam_WithInvalidData_ShouldFailValidation()
    {
        // Arrange
        CreateFloorballTeamCommand command = new CreateFloorballTeamCommand("", "Description", Guid.NewGuid(), "InvalidDivision");

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(command));
        exception.Errors.Should().NotBeEmpty();
    }
}
```

## 📋 Application Development Checklist

### Design Phase
- [ ] CQRS operation type identified (Command vs Query)
- [ ] Input/output DTOs designed
- [ ] Validation rules defined
- [ ] Business logic requirements understood
- [ ] Error handling scenarios planned

### Implementation Phase
- [ ] Command/Query classes implemented
- [ ] Validators implemented with comprehensive rules
- [ ] Handlers implemented with proper error handling
- [ ] DTOs and mapping configurations created
- [ ] Pipeline behaviors considered (caching, logging)

### Testing Phase
- [ ] Unit tests for handlers (success and failure cases)
- [ ] Unit tests for validators (all validation rules)
- [ ] Integration tests for end-to-end scenarios
- [ ] Performance tests for query operations
- [ ] Caching behavior validation

### Documentation Phase
- [ ] API documentation updated
- [ ] Validation rules documented
- [ ] Error codes and messages documented
- [ ] Performance characteristics noted

### Quality Phase
- [ ] Code review completed
- [ ] Performance benchmarks verified
- [ ] Error handling tested thoroughly
- [ ] Logging statements appropriate
- [ ] Security considerations addressed

## 🔧 Common Patterns & Examples

### Adding New Sport Features
1. Create sport-specific command/query folders
2. Implement sport-specific DTOs
3. Create sport-specific validators
4. Implement handlers following existing patterns
5. Add mapping configurations
6. Register new validators in DI

### Adding Cross-Cutting Behaviors
1. Implement IPipelineBehavior<TRequest, TResponse>
2. Register behavior in DI configuration
3. Consider behavior order (validation typically first)
4. Add comprehensive logging
5. Handle exceptions appropriately

### Adding Caching
1. Identify cacheable queries
2. Generate appropriate cache keys
3. Set appropriate expiration policies
4. Implement cache invalidation strategies
5. Monitor cache hit rates

## ⚠️ Common Pitfalls to Avoid

1. **Mixing Commands and Queries** - Maintain strict CQRS separation
2. **Heavy Command Handlers** - Keep handlers focused on orchestration
3. **Missing Validation** - Always validate all inputs
4. **Ignoring Async** - Use async/await throughout
5. **Poor Error Handling** - Use Result pattern consistently
6. **Over-Caching** - Cache only appropriate queries
7. **Tight Coupling** - Keep handlers independent
8. **Missing Logging** - Log important operations and errors

## 📊 Performance Considerations

### Query Optimization
- Use projection to return only required fields
- Implement pagination for large datasets
- Cache frequently accessed data
- Monitor query execution times

### Command Performance
- Minimize database round trips
- Use bulk operations when appropriate
- Implement proper transaction boundaries
- Consider async processing for heavy operations

### Memory Management
- Dispose resources properly
- Avoid memory leaks in long-running operations
- Use streaming for large data transfers
- Monitor garbage collection patterns

## 📚 Additional Resources

- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [CQRS Journey](https://docs.microsoft.com/en-us/previous-versions/msp-n-p/jj554200(v=pandp.10))
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Result Pattern](https://enterprisecraftsmanship.com/posts/functional-c-handling-failures-input-errors/) 