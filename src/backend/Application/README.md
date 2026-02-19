# Application Layer - League Management System

## Overview

This is the **Application Layer** of the league management system, implementing the **CQRS (Command Query Responsibility Segregation)** pattern using **MediatR**. This layer orchestrates business workflows, handles user requests, validates input, and coordinates between the Domain and Infrastructure layers while maintaining clean separation of concerns.

## 🏗️ Architecture

The Application layer follows **Clean Architecture**, **CQRS**, and **vertical slices (feature-based)** structure:

- **Features** – Each feature (Auth, Common/Clubs, Floorball/Teams, etc.) is a vertical slice containing Commands, Queries, Handlers, DTOs, Mappings, and Validators for that capability.
- **Commands** – Write operations and business state changes (per feature).
- **Queries** – Read operations and data retrieval (per feature).
- **Handlers** – Process commands and queries with business logic.
- **Validators** – Input validation using FluentValidation (per request type).
- **DTOs** – Data transfer objects; feature-specific under `Features/`, shared under `Features/Common/Shared`.
- **Behaviors** – Cross-cutting concerns via MediatR pipeline (validation, logging).
- **Result Pattern** – Consistent error handling and response structure (`Result<T>`, `PagedResult<T>`).

## 🚀 Technology Stack

- **.NET 9.0** - Latest .NET framework
- **MediatR 12.5** - Mediator pattern implementation for CQRS
- **FluentValidation 12.0** - Powerful validation framework
- **Microsoft.Extensions.DependencyInjection** - Dependency injection
- **Microsoft.Extensions.Logging** - Structured logging

## 📁 Project Structure

The Application layer is organized by **features**: each feature is self-contained with its own Commands, Queries, Handlers, DTOs, Mappings, and Validators. Shared and cross-cutting pieces live at the project root.

```
Application/
├── Features/                           # Feature-based CQRS (primary structure)
│   ├── Auth/                          # Passwordless auth (login code, JWT, refresh/revoke)
│   │   ├── Commands/
│   │   ├── DTOs/
│   │   ├── Handlers/
│   │   └── Validators/
│   ├── Common/                        # Cross-domain features
│   │   ├── Clubs/                     # Club CRUD and queries
│   │   ├── Divisions/                 # Division CRUD and queries
│   │   ├── Images/                    # Image deletion
│   │   ├── MatchTimer/                # Match timer (start, stop, reset, elapsed)
│   │   ├── News/                      # News articles and categories
│   │   ├── Persons/                   # Person CRUD and search
│   │   ├── Search/                    # Global search
│   │   ├── Shared/                    # Shared DTOs (e.g. PagedResult)
│   │   └── Users/                     # User CRUD and lookup
│   └── Floorball/                     # Floorball sport domain
│       ├── Constants/
│       ├── Matches/                   # Matches, periods, start/complete
│       ├── Players/                   # Floorball players
│       ├── Referees/                  # Referee CRUD
│       ├── Seasons/                   # Season mappings
│       ├── Statistics/                # Match/season statistics
│       ├── TeamManagers/              # Team manager CRUD
│       └── Teams/                     # Teams, rosters, divisions
│
├── Behaviors/                         # MediatR pipeline (validation, logging)
├── Common/                            # Shared application logic
│   └── Result.cs                      # Result pattern
├── Configuration/                     # App configuration classes
│   ├── JwtConfiguration.cs
│   ├── LoginCodeConfiguration.cs
│   ├── AzureCommunicationServicesConfiguration.cs
│   └── SeedConfiguration.cs
├── DependencyInjections/              # Service registration
├── Interfaces/                        # Service abstractions (e.g. Auth)
│   └── Auth/
│       ├── IEmailService.cs
│       └── IJwtTokenService.cs
└── Application.csproj
```

**Per-feature layout** (e.g. `Features/Common/Clubs`, `Features/Floorball/Teams`):

- **Commands/** – Write operations (`IRequest<Result<T>>`)
- **Queries/** – Read operations (`IRequest<Result<T>>` or `IRequest<PagedResult<T>>`)
- **Handlers/** – Command and query handlers
- **DTOs/** – Data transfer objects for the feature
- **Mappings/** – Entity ↔ DTO mapping (where used)
- **Validators/** – FluentValidation validators for commands/queries

Not every feature has every folder (e.g. Auth has no Queries; Search may have only Queries + Handler).

## 🎯 Core Components

### CQRS Implementation
- **Commands** - Represent user intentions to modify system state
- **Queries** - Represent requests for data without side effects
- **Handlers** - Process commands and queries independently
- **Separation** - Clear distinction between read and write operations

### MediatR Integration
- **Request/Response** - Type-safe request handling
- **Pipeline Behaviors** - Cross-cutting concerns (validation, logging, caching)
- **Notification** - Domain event handling via notifications
- **Decoupling** - Loose coupling between controllers and business logic

### Validation Framework
- **FluentValidation** - Declarative validation rules
- **Pipeline Integration** - Automatic validation before handler execution
- **Error Aggregation** - Collect and return all validation errors
- **Type Safety** - Strongly-typed validation rules

### Result Pattern
- **Consistent Responses** - Standardized success/failure responses
- **Error Handling** - Rich error information and status codes
- **Type Safety** - Strongly-typed result objects
- **Functional Style** - Railway-oriented programming approach

## 🔄 CQRS Pattern Details

### Commands (Write Side)
Commands represent user intentions to change system state:

```csharp
public record CreateClubCommand(
    string Name,
    string Description,
    Address Address
) : IRequest<Result<ClubDto>>;
```

### Queries (Read Side)
Queries represent requests for data without side effects:

```csharp
public record GetClubByIdQuery(
    Guid ClubId
) : IRequest<Result<ClubDto>>;

public record GetClubsQuery(
    int Page = 1,
    int PageSize = 50,
    string? SearchTerm = null
) : IRequest<Result<PagedResult<ClubDto>>>;
```

### Handlers
Process commands and queries with business logic:

```csharp
public class CreateClubCommandHandler : IRequestHandler<CreateClubCommand, Result<ClubDto>>
{
    // Implementation coordinates domain operations
}
```

## 🛡️ Cross-Cutting Concerns

### Validation Pipeline
- **Automatic Validation** - All requests validated before processing
- **Fluent Rules** - Declarative validation rule definitions
- **Error Collection** - Aggregated validation error responses
- **Performance** - Fast validation with early exit on failures

### Error Handling
- **Result Pattern** - Consistent error response structure
- **Exception Management** - Graceful exception handling
- **Logging Integration** - Comprehensive error logging
- **Client-Friendly** - Meaningful error messages for UI

### Performance Optimization
- **Async Operations** - Non-blocking I/O throughout
- **Caching Strategies** - Query result caching capabilities
- **Pagination** - Efficient data retrieval for large datasets
- **Projection** - Return only required data fields

## 🔧 Validation Framework

### FluentValidation Integration
The application uses FluentValidation for comprehensive input validation:

```csharp
public class CreateClubCommandValidator : AbstractValidator<CreateClubCommand>
{
    public CreateClubCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
            
        RuleFor(x => x.Address)
            .NotNull()
            .SetValidator(new AddressValidator());
    }
}
```

### Validation Behaviors
Automatic validation pipeline ensures all requests are validated:

- **Pre-Execution** - Validation runs before handler execution
- **Error Aggregation** - Collects all validation failures
- **Performance** - Fast-fail validation for better performance
- **Consistency** - Uniform validation across all endpoints

## 📊 Data Transfer Objects (DTOs)

### Response DTOs
Standardized data contracts for external communication:

```csharp
public record ClubDto(
    Guid Id,
    string Name,
    string Description,
    AddressDto Address,
    DateTime CreatedAt
);

public record AddressDto(
    string Street,
    string City,
    string PostalCode,
    string Country
);
```

### Paged Results
Consistent pagination across all query responses:

```csharp
public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
```

## 🎯 Handlers Pattern

### Command Handlers
Handle business state changes:

```csharp
public class CreateClubCommandHandler : IRequestHandler<CreateClubCommand, Result<ClubDto>>
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public async Task<Result<ClubDto>> Handle(CreateClubCommand request, CancellationToken cancellationToken)
    {
        // 1. Create domain entity
        // 2. Persist via repository
        // 3. Commit transaction
        // 4. Map to DTO
        // 5. Return result
    }
}
```

### Query Handlers
Handle data retrieval:

```csharp
public class GetClubByIdQueryHandler : IRequestHandler<GetClubByIdQuery, Result<ClubDto>>
{
    private readonly IClubRepository _clubRepository;
    private readonly IMapper _mapper;

    public async Task<Result<ClubDto>> Handle(GetClubByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Retrieve from repository
        // 2. Map to DTO
        // 3. Return result
    }
}
```

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK
- Understanding of CQRS and MediatR patterns
- Familiarity with FluentValidation

### Service Registration
```csharp
// In Program.cs
builder.Services.AddApplication();
```

### Usage in Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
public class ClubsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClubsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ClubDto>> CreateClub(CreateClubCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
```

## 🔍 Pipeline Behaviors

### Validation Behavior
Automatically validates requests before handler execution:

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    // Validates request and aggregates errors
    // Fails fast on validation errors
    // Logs validation failures
}
```

### Logging Behavior
Comprehensive request/response logging:

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    // Logs request details
    // Measures execution time
    // Logs response or errors
}
```

## 📈 Performance Considerations

### Async/Await Pattern
- All operations are asynchronous
- Non-blocking I/O throughout the pipeline
- Proper cancellation token usage
- Memory-efficient operations

### Caching Strategy
- Query result caching for frequently accessed data
- Cache invalidation on domain events
- Configurable cache duration
- Memory and distributed caching support

### Pagination
- Efficient large dataset handling
- Configurable page sizes
- Total count optimization
- Skip/Take query optimization

## 🛡️ Security & Authentication

### Passwordless Authentication Commands
The authentication flow lives under **`Features/Auth/`** and is implemented as CQRS commands:

| Command | Description |
|---------|-------------|
| `RequestLoginCodeCommand` | Generates a 6-digit code (via `RandomNumberGenerator`), stores it on the User entity, and sends it via `IEmailService` |
| `VerifyLoginCodeCommand` | Validates the code, enforces brute-force protection (max 5 attempts), and returns JWT access token + refresh token |
| `RefreshTokenCommand` | Performs refresh token rotation -- validates the existing token, revokes it, issues a new pair; detects token theft |
| `RevokeTokenCommand` | Revokes a refresh token (used for logout) |

### Service Interfaces
| Interface | Description |
|-----------|-------------|
| `IEmailService` | Abstraction for sending login codes (console in dev, Azure Email in prod) |
| `IJwtTokenService` | Generates JWT access tokens, refresh tokens, and token hashes |

### Configuration Classes
| Class | Config Section | Description |
|-------|---------------|-------------|
| `JwtConfiguration` | `Jwt` | Issuer, audience, secret key, token lifetimes |
| `LoginCodeConfiguration` | `LoginCode` | Code length (6), expiration (10 min), max attempts (5) |
| `AzureCommunicationServicesConfiguration` | `AzureCommunicationServices` | Connection string and sender address for Azure Email |
| `SeedConfiguration` | `Seed` | Admin email for initial user creation |

### Input Validation
- Comprehensive validation rules for all auth commands
- SQL injection prevention
- XSS attack mitigation
- Data sanitization

### Authorization
- Role-based access control integration
- Resource-based authorization
- Command/query level security
- Audit trail capabilities

## 🧪 Testing Strategy

### Unit Testing
- Handler logic testing
- Validation rule testing
- Mapping configuration testing
- Behavior pipeline testing

### Integration Testing
- End-to-end request processing
- Database integration testing
- External service integration
- Performance testing

## 📚 Learn More

- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [Clean Architecture Guidelines](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## 🤝 Contributing

When contributing to the Application layer:

1. **Use the feature structure** – Add or extend code under `Features/<Area>/<FeatureName>/` (Commands, Queries, Handlers, DTOs, Mappings, Validators as needed). Shared DTOs go in `Features/Common/Shared/DTOs`.
2. Follow CQRS principles strictly within each feature.
3. Implement comprehensive validation rules (FluentValidation per command/query).
4. Use the Result pattern consistently (`Result<T>`, `PagedResult<T>`).
5. Write thorough unit tests for handlers and validators.
6. Document public APIs and consider performance implications.

## 📄 License

This project is part of the League Management System application. 