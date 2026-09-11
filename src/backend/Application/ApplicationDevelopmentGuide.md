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
1. **Feature-Based Structure** — All feature code lives under `Features/<Area>/<FeatureName>/`
2. **CQRS Separation** — Strict separation between commands and queries (each in its own file/folder)
3. **Single Responsibility** — Each handler has one specific purpose
4. **Validation First** — All input must be validated before processing (FluentValidation)
5. **Result Pattern** — Consistent success/failure response via `Result<T>` (access `.Data`, `.IsSuccess`, `.Error`)
6. **Static Mappers** — Entity ↔ DTO mapping via static classes, not AutoMapper
7. **Async Operations** — All operations should be asynchronous

## 🚀 Development Process

### Step 1: Feature Analysis & Design

#### 1.1 Understand the Business Requirement
- [ ] Identify if this is a command (write) or query (read) operation
- [ ] Determine required input parameters and validation rules
- [ ] Define expected output format and DTOs
- [ ] Understand business rules and domain constraints
- [ ] Plan error handling scenarios

#### 1.2 Application Design
- [ ] Decide which feature area this belongs to (`Features/Common/...` or `Features/Floorball/...`)
- [ ] Design command/query record structure
- [ ] Plan DTO shape and static mapper methods
- [ ] Design validation rules
- [ ] Plan handler implementation and required repositories

#### 1.3 Implementation Planning
- [ ] Plan service dependencies
- [ ] Design unit tests
- [ ] Consider performance implications
- [ ] Plan integration points

### Step 2: Implementation

#### 2.1 Commands (Write Operations)

Each command, its validator, and its handler live under the same feature folder. For example, creating a floorball team involves files under `Features/Floorball/Teams/`:

**Command** (`Features/Floorball/Teams/Commands/CreateFloorballTeamCommand.cs`):
```csharp
using Application.Common;
using Application.Features.Floorball.Teams.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Floorball.Teams.Commands;

public record CreateFloorballTeamCommand(
    string Name,
    Guid? DivisionId,
    Guid ClubId,
    string HomeArena,
    string PrimaryJerseyColor,
    TeamCategory TeamCategory,
    string? SecondaryJerseyColor,
    string? ShortName) : IRequest<Result<FloorballTeamDto>>;
```

**Validator** (`Features/Floorball/Teams/Validators/CreateFloorballTeamCommandValidator.cs`):
```csharp
using Application.Features.Floorball.Teams.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Teams.Validators;

public class CreateFloorballTeamCommandValidator : AbstractValidator<CreateFloorballTeamCommand>
{
    public CreateFloorballTeamCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Team name is required")
            .MaximumLength(100).WithMessage("Team name cannot exceed 100 characters");

        RuleFor(x => x.ClubId)
            .NotEmpty().WithMessage("Club ID is required")
            .NotEqual(Guid.Empty).WithMessage("Club ID cannot be empty");

        RuleFor(x => x.HomeArena)
            .NotEmpty().WithMessage("Home arena is required")
            .MaximumLength(100).WithMessage("Home arena name cannot exceed 100 characters");

        RuleFor(x => x.PrimaryJerseyColor)
            .NotEmpty().WithMessage("Primary jersey color is required")
            .MaximumLength(50).WithMessage("Primary jersey color cannot exceed 50 characters");

        RuleFor(x => x.TeamCategory)
            .NotNull().WithMessage("Team category is required")
            .IsInEnum().WithMessage("Invalid team category value");
    }
}
```

**Handler** (`Features/Floorball/Teams/Handlers/CreateFloorballTeamHandler.cs`):
```csharp
using Application.Features.Floorball.Teams.Commands;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Teams.Mappings;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Application.Features.Floorball.Teams.Handlers;

public class CreateFloorballTeamHandler : IRequestHandler<CreateFloorballTeamCommand, Result<FloorballTeamDto>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<CreateFloorballTeamHandler> _logger;

    public CreateFloorballTeamHandler(
        IFloorballTeamRepository teamRepository,
        IClubRepository clubRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<CreateFloorballTeamHandler> logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballTeamDto>> Handle(
        CreateFloorballTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Club? club = await _clubRepository.GetByIdAsync(request.ClubId);
            if (club == null)
                return Result<FloorballTeamDto>.Failure("Club not found");

            FloorballTeam team = FloorballTeamMapper.ToEntity(request, club);

            _logger.LogInformation("Creating new floorball team: {TeamName}", request.Name);
            await _teamRepository.AddAsync(team);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballTeamDto teamDto = FloorballTeamMapper.ToDto(team, club);
            _logger.LogInformation("Successfully created floorball team with ID: {TeamId}", team.Id);

            return Result<FloorballTeamDto>.Success(teamDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating floorball team: {TeamName}", request.Name);
            return Result<FloorballTeamDto>.Failure("An error occurred while creating the floorball team.");
        }
    }
}
```

#### 2.2 Queries (Read Operations)

Queries follow the same feature structure. Files live under the feature's `Queries/` and `Handlers/` folders.

**Query** (`Features/Floorball/Teams/Queries/GetAllFloorballTeamsQuery.cs`):
```csharp
using Application.Common;
using Application.Features.Floorball.Teams.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Floorball.Teams.Queries;

public record GetAllFloorballTeamsQuery(
    int Page = 1,
    int PageSize = 0,
    Guid? ClubId = null,
    string? Division = null
) : IRequest<Result<PagedResult<FloorballTeamDto>>>
{
    public const string ResourceKey = "FloorballTeams";
}
```

**Query Validator** (`Features/Floorball/Teams/Validators/GetAllFloorballTeamsQueryValidator.cs`):
```csharp
using Application.Features.Floorball.Teams.Queries;
using FluentValidation;

namespace Application.Features.Floorball.Teams.Validators;

public class GetAllFloorballTeamsQueryValidator : AbstractValidator<GetAllFloorballTeamsQuery>
{
    public GetAllFloorballTeamsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(0).WithMessage("Page size must be 0 (default) or greater");
    }
}
```

**Query Handler** (`Features/Floorball/Teams/Handlers/GetAllFloorballTeamsHandler.cs`):
```csharp
using Application.Features.Floorball.Teams.Queries;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Teams.Mappings;
using Application.Common;
using Domain.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Application.Features.Floorball.Teams.Handlers;

public class GetAllFloorballTeamsHandler
    : IRequestHandler<GetAllFloorballTeamsQuery, Result<PagedResult<FloorballTeamDto>>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetAllFloorballTeamsHandler> _logger;

    public GetAllFloorballTeamsHandler(
        IFloorballTeamRepository teamRepository,
        IClubRepository clubRepository,
        ILogger<GetAllFloorballTeamsHandler> logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<FloorballTeamDto>>> Handle(
        GetAllFloorballTeamsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Retrieve teams from repository (with pagination)
            IEnumerable<FloorballTeam> teams = await _teamRepository.GetAllAsync();

            // Load related clubs
            List<Guid> clubIds = teams.Select(t => t.ClubId).Distinct().ToList();
            Dictionary<Guid, Club> clubs = /* load clubs by IDs */;

            // Map using static mapper
            IEnumerable<FloorballTeamDto> teamDtos = FloorballTeamMapper.ToDtos(teams, clubs);

            PagedResult<FloorballTeamDto> result = new PagedResult<FloorballTeamDto>(
                teamDtos, teamDtos.Count(), request.Page, request.PageSize);

            return Result<PagedResult<FloorballTeamDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving floorball teams");
            return Result<PagedResult<FloorballTeamDto>>.Failure("An error occurred while retrieving teams");
        }
    }
}
```

#### 2.3 Data Transfer Objects (DTOs)

DTOs live inside each feature's `DTOs/` folder. Shared DTOs (e.g. `PagedResult`, `ClubDto`) live in `Features/Common/Shared/DTOs/`.

**Feature DTO** (`Features/Floorball/Teams/DTOs/FloorballTeamDto.cs`):
```csharp
using Application.Features.Common.Clubs.DTOs;

namespace Application.Features.Floorball.Teams.DTOs;

public record FloorballTeamDto(
    Guid Id,
    string Name,
    string ShortName,
    Guid? DivisionId,
    ClubDto Club,
    string HomeArena,
    string PrimaryJerseyColor,
    string SecondaryJerseyColor,
    string? LogoUrl,
    bool HasActiveMembers,
    IReadOnlyCollection<FloorballTeamPlayerDto> Roster);

public record FloorballTeamSummaryDto(
    Guid Id,
    string Name,
    Guid? DivisionId,
    ClubDto Club,
    string HomeArena,
    string PrimaryJerseyColor,
    string SecondaryJerseyColor,
    string? LogoUrl,
    bool HasActiveMembers,
    TeamCategory TeamCategory);
```

#### 2.4 Static Mapper Classes

The project uses **static mapper classes** (not AutoMapper) for entity ↔ DTO mapping. Each feature's `Mappings/` folder contains a mapper with `ToDto()`, `ToDtos()`, `ToEntity()`, and `UpdateFromCommand()` methods.

**Mapper** (`Features/Floorball/Teams/Mappings/FloorballTeamMapper.cs`):
```csharp
using Application.Features.Floorball.Teams.Commands;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Common.Clubs.Mappings;
using Domain.Entities.Common;
using Domain.Entities.Floorball;

namespace Application.Features.Floorball.Teams.Mappings;

public static class FloorballTeamMapper
{
    public static FloorballTeamDto ToDto(FloorballTeam team, Club club,
        Dictionary<Guid, Person>? playerPersons = null)
    {
        return new FloorballTeamDto(
            team.Id,
            team.Name,
            team.ShortName,
            team.DivisionId,
            ClubMapper.ToDto(club),
            team.HomeArena,
            team.PrimaryJerseyColor,
            team.SecondaryJerseyColor,
            team.GetEffectiveLogoUrl(club.LogoUrl)?.ToString(),
            team.HasActiveMembers,
            /* map roster */);
    }

    public static IEnumerable<FloorballTeamDto> ToDtos(
        IEnumerable<FloorballTeam> teams,
        Dictionary<Guid, Club>? clubs = null,
        Dictionary<Guid, Person>? playerPersons = null) { ... }

    public static FloorballTeam ToEntity(CreateFloorballTeamCommand command, Club club)
    {
        return new FloorballTeam(
            command.Name,
            command.DivisionId,
            club,
            command.HomeArena,
            command.PrimaryJerseyColor,
            command.TeamCategory,
            command.SecondaryJerseyColor,
            command.ShortName);
    }

    public static void UpdateFromCommand(FloorballTeam team, UpdateFloorballTeamCommand command)
    {
        team.UpdateName(command.Name);
        team.UpdateDivision(command.DivisionId);
        team.UpdateHomeArena(command.HomeArena);
        team.UpdateJerseyColors(command.PrimaryJerseyColor, command.SecondaryJerseyColor!);
        team.UpdateShortName(command.ShortName);
    }

    public static FloorballTeamSummaryDto ToSummaryDto(FloorballTeam team, Club club) { ... }
}
```

**Key conventions:**
- `ToDto(entity, ...) → DTO` for single entity mapping.
- `ToDtos(entities, ...) → IEnumerable<DTO>` for collection mapping (accepts dictionaries of related entities).
- `ToEntity(command, ...) → Entity` for creating new domain entities from commands.
- `UpdateFromCommand(entity, command)` for updating existing entities from commands.
- Related entities (Club, Person, etc.) are passed as parameters since navigation properties may not be loaded via EF.

#### 2.5 Pipeline Behaviors

The project registers two MediatR pipeline behaviors (in `Behaviors/`). They run in order for every request: **Logging → Validation**.

**LoggingBehavior** (`Behaviors/LoggingBehavior.cs`) — logs every request with timing and structured context:
```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        string requestId = Guid.NewGuid().ToString();

        _logger.LogInformation("Starting request {RequestName} with ID {RequestId}", requestName, requestId);

        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            TResponse response = await next();
            stopwatch.Stop();

            _logger.LogInformation("Completed {RequestName} ({RequestId}) in {ElapsedMs}ms",
                requestName, requestId, stopwatch.ElapsedMilliseconds);

            if (stopwatch.ElapsedMilliseconds > 1000)
                _logger.LogWarning("Slow request: {RequestName} took {ElapsedMs}ms",
                    requestName, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Request {RequestName} ({RequestId}) failed after {ElapsedMs}ms",
                requestName, requestId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
```

**ValidationBehavior** (`Behaviors/ValidationBehaviors.cs`) — runs FluentValidation validators before the handler. Returns `Result<T>.ValidationFailure(...)` instead of throwing:
```csharp
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Application.Common;

namespace Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_validators.Any())
            return await next();

        ValidationContext<TRequest> context = new(request);
        ValidationResult[] results = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        List<ValidationFailure> failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
            return CreateValidationFailureResult(failures);

        return await next();
    }

    // Returns Result<T>.ValidationFailure(...) when TResponse is Result<T>,
    // or throws ValidationException as a fallback.
    private static TResponse CreateValidationFailureResult(List<ValidationFailure> failures) { ... }
}
```

#### 2.6 Service Registration

All MediatR handlers, validators, and behaviors are discovered via assembly scanning. Static mappers don't need registration.

**DI** (`DependencyInjections/DependencyInjection.cs`):
```csharp
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Application.Behaviors;
using Application.Features.Common.MatchTimer.Services;
using Application.Services.Common;
using MediatR;
using FluentValidation;

namespace Application.DependencyInjections;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Pipeline order: Logging → Validation
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<IPaginationService, PaginationService>();
        services.AddScoped<IMatchTimerService, PersistentMatchTimerService>();

        return services;
    }
}
```

> **Note:** The project does **not** use AutoMapper or IMemoryCache. Mapping is done via static mapper classes, and there is no caching pipeline behavior.

### Step 3: Testing

#### 3.1 Command Handler Tests

Since mapping is done via static methods, handlers only need repository and unit-of-work mocks (no `IMapper` mock):

```csharp
using Application.Features.Floorball.Teams.Commands;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Teams.Handlers;

public class CreateFloorballTeamHandlerTests
{
    private readonly Mock<IFloorballTeamRepository> _teamRepositoryMock;
    private readonly Mock<IClubRepository> _clubRepositoryMock;
    private readonly Mock<IFloorballUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CreateFloorballTeamHandler>> _loggerMock;
    private readonly CreateFloorballTeamHandler _handler;

    public CreateFloorballTeamHandlerTests()
    {
        _teamRepositoryMock = new Mock<IFloorballTeamRepository>();
        _clubRepositoryMock = new Mock<IClubRepository>();
        _unitOfWorkMock = new Mock<IFloorballUnitOfWork>();
        _loggerMock = new Mock<ILogger<CreateFloorballTeamHandler>>();

        _handler = new CreateFloorballTeamHandler(
            _teamRepositoryMock.Object,
            _clubRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        Guid clubId = Guid.NewGuid();
        Club club = new Club("Test Club", "Description");
        CreateFloorballTeamCommand command = new("Test Team", null, clubId, "Arena",
            "Blue", TeamCategory.Men, null, null);

        _clubRepositoryMock
            .Setup(x => x.GetByIdAsync(clubId))
            .ReturnsAsync(club);

        // Act
        Result<FloorballTeamDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Test Team");

        _teamRepositoryMock.Verify(x => x.AddAsync(It.IsAny<FloorballTeam>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentClub_ShouldReturnFailure()
    {
        // Arrange
        CreateFloorballTeamCommand command = new("Team", null, Guid.NewGuid(), "Arena",
            "Blue", TeamCategory.Men, null, null);

        _clubRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Club?)null);

        // Act
        Result<FloorballTeamDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Club not found");
        _teamRepositoryMock.Verify(x => x.AddAsync(It.IsAny<FloorballTeam>()), Times.Never);
    }
}
```

#### 3.2 Validator Tests

```csharp
using Application.Features.Floorball.Teams.Commands;
using Application.Features.Floorball.Teams.Validators;

public class CreateFloorballTeamCommandValidatorTests
{
    private readonly CreateFloorballTeamCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        CreateFloorballTeamCommand command = new("Test Team", null, Guid.NewGuid(), "Arena",
            "Blue", TeamCategory.Men, null, null);

        ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithInvalidName_ShouldFail(string? name)
    {
        CreateFloorballTeamCommand command = new(name!, null, Guid.NewGuid(), "Arena",
            "Blue", TeamCategory.Men, null, null);

        ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFloorballTeamCommand.Name));
    }

    [Fact]
    public void Validate_WithEmptyClubId_ShouldFail()
    {
        CreateFloorballTeamCommand command = new("Team", null, Guid.Empty, "Arena",
            "Blue", TeamCategory.Men, null, null);

        ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFloorballTeamCommand.ClubId));
    }
}
```

#### 3.3 Integration Tests

```csharp
using Application.Features.Floorball.Teams.Commands;
using Application.Features.Floorball.Teams.Queries;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Common.Clubs.Commands;

public class FloorballTeamIntegrationTests : IClassFixture<ApplicationTestFixture>, IDisposable
{
    private readonly IServiceScope _scope;
    private readonly IMediator _mediator;

    public FloorballTeamIntegrationTests(ApplicationTestFixture fixture)
    {
        _scope = fixture.ServiceProvider.CreateScope();
        _mediator = _scope.ServiceProvider.GetRequiredService<IMediator>();
    }

    public void Dispose() => _scope.Dispose();

    [Fact]
    public async Task CreateAndRetrieve_ShouldWorkEndToEnd()
    {
        // Arrange — create a club first
        CreateClubCommand clubCmd = new("Test Club", "Description");
        Result<ClubDto> clubResult = await _mediator.Send(clubCmd);
        clubResult.IsSuccess.Should().BeTrue();

        CreateFloorballTeamCommand teamCmd = new("Test Team", null, clubResult.Data!.Id,
            "Arena", "Blue", TeamCategory.Men, null, null);

        // Act
        Result<FloorballTeamDto> createResult = await _mediator.Send(teamCmd);

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        createResult.Data!.Name.Should().Be("Test Team");

        GetFloorballTeamByIdQuery getQuery = new(createResult.Data.Id);
        Result<FloorballTeamDto> getResult = await _mediator.Send(getQuery);

        getResult.IsSuccess.Should().BeTrue();
        getResult.Data!.Name.Should().Be("Test Team");
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
- [ ] Feature folder created under `Features/<Area>/<FeatureName>/`
- [ ] Command/Query record classes implemented in `Commands/` or `Queries/`
- [ ] Validators implemented in `Validators/` with comprehensive rules
- [ ] Handlers implemented in `Handlers/` with proper error handling
- [ ] DTOs created in `DTOs/`
- [ ] Static mapper class created in `Mappings/` with `ToDto()`, `ToEntity()`, etc.

### Testing Phase
- [ ] Unit tests for handlers (success and failure cases)
- [ ] Unit tests for validators (all validation rules)
- [ ] Integration tests for end-to-end scenarios
- [ ] Performance tests for query operations

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

### Adding a New Sport (e.g. Ice Hockey)
1. Create the feature folder structure under `Features/`:
   ```
   Features/IceHockey/
   ├── Teams/
   │   ├── Commands/
   │   ├── Queries/
   │   ├── Handlers/
   │   ├── DTOs/
   │   ├── Mappings/
   │   └── Validators/
   ├── Players/
   │   └── ...
   ├── Matches/
   │   └── ...
   └── Constants/
   ```
2. Implement record-based **Commands** and **Queries** (`IRequest<Result<T>>`).
3. Create **DTOs** in each feature's `DTOs/` folder.
4. Implement **static mapper** classes in `Mappings/` with `ToDto()`, `ToEntity()`, `UpdateFromCommand()`.
5. Write **FluentValidation validators** in `Validators/`.
6. Implement **Handlers** in `Handlers/` — inject repositories and unit-of-work; use the static mapper.
7. No registration needed — MediatR and FluentValidation auto-discover from the assembly.

### Adding a New Common Feature
Place it under `Features/Common/<FeatureName>/` (e.g. `Features/Common/Venues/`). Shared DTOs that multiple features reference go in `Features/Common/Shared/DTOs/`.

### Adding Cross-Cutting Behaviors
1. Implement `IPipelineBehavior<TRequest, TResponse>` in `Behaviors/`.
2. Register the behavior in `DependencyInjection.cs` via `cfg.AddBehavior(...)`.
3. Pipeline order matters — current order is Logging → Validation.

## ⚠️ Common Pitfalls to Avoid

1. **Placing files in the wrong namespace** — Commands, Queries, Handlers, DTOs, Mappings, and Validators must live under `Features/<Area>/<Feature>/<SubFolder>`.
2. **Mixing Commands and Queries** — Maintain strict CQRS separation.
3. **Heavy Command Handlers** — Keep handlers focused on orchestration; complex logic belongs in the domain.
4. **Missing Validation** — Every command/query should have a FluentValidation validator.
5. **Using AutoMapper or IMapper** — The project uses static mapper classes, not AutoMapper.
6. **Ignoring Async** — Use async/await throughout.
7. **Not using the Result pattern** — Return `Result<T>.Success()` / `Result<T>.Failure()` consistently; don't throw exceptions for business failures.
8. **Accessing `.Data` without checking `.IsSuccess`** — Always check the result before using `.Data`.
9. **Missing Logging** — Log important operations and errors in handlers.

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