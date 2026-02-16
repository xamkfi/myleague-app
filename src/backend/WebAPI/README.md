# WebAPI - League Management System

## 🎯 Overview

The WebAPI layer serves as the presentation layer of the League Management System, providing a RESTful HTTP interface built with ASP.NET Core 9.0. This layer follows Clean Architecture principles and implements the API Gateway pattern, acting as the entry point for all client applications including web frontends, mobile apps, and third-party integrations.

### Key Responsibilities
- **HTTP Request Handling** - Processing incoming HTTP requests and routing to appropriate handlers
- **Response Formatting** - Standardizing API responses with consistent structure and error handling
- **Input Validation** - Validating request models and handling validation errors
- **Authentication & Authorization** - Passwordless email authentication with JWT and refresh tokens
- **API Documentation** - Providing interactive documentation through Scalar/OpenAPI
- **Cross-Cutting Concerns** - Implementing logging, CORS, exception handling, and health checks

## 🏗️ Architecture & Design Principles

### Clean Architecture Implementation
```
┌─────────────────────────────────────────────────────┐
│                  WebAPI Layer                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  │
│  │ Controllers │  │ Middlewares │  │ Extensions  │  │
│  └─────────────┘  └─────────────┘  └─────────────┘  │
│           │              │              │           │
└───────────┼──────────────┼──────────────┼───────────┘
            │              │              │
┌───────────▼──────────────▼──────────────▼───────────┐
│              Application Layer (MediatR)            │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  │
│  │  Commands   │  │   Queries   │  │  Handlers   │  │
│  └─────────────┘  └─────────────┘  └─────────────┘  │
└─────────────────────────────────────────────────────┘
```

### Core Design Patterns
- **API Gateway Pattern** - Single entry point for all client requests
- **CQRS (Command Query Responsibility Segregation)** - Separation of read and write operations
- **Mediator Pattern** - Decoupled request handling through MediatR
- **Repository Pattern** - Data access abstraction (via Application layer)
- **Result Pattern** - Consistent success/failure response handling

## 🛠️ Technology Stack

### Core Technologies
- **.NET 9.0** - Latest .NET platform with enhanced performance
- **ASP.NET Core 9.0** - Web framework with minimal APIs support
- **C# 13** - Modern language features and nullable reference types

### Key Packages
- **MediatR 12.5** - Mediator pattern implementation for CQRS
- **FluentValidation.AspNetCore 11.3** - Request validation and error handling
- **Microsoft.AspNetCore.Authentication.JwtBearer 9.0** - JWT bearer token authentication
- **Serilog.AspNetCore 9.0** - Structured logging with multiple sinks
- **Scalar.AspNetCore 1.2** - Modern OpenAPI documentation interface
- **Microsoft.AspNetCore.OpenApi 9.0** - OpenAPI specification generation

### Documentation & API Tools
- **Scalar UI** - Interactive API documentation with modern interface
- **OpenAPI 3.0** - API specification standard
- **XML Documentation** - Code documentation generation

### Logging Infrastructure
- **Serilog.Sinks.Console** - Console output for development
- **Serilog.Sinks.File** - File-based logging for persistence
- **Serilog.Sinks.Seq** - Structured log aggregation (when configured)

## 📁 Project Structure

```
WebAPI/
├── Controllers/              # API Controllers
│   ├── Auth/                # Authentication controllers
│   │   └── AuthController.cs
│   └── Common/              # Common entity controllers
│       ├── ClubsController.cs
│       └── UsersController.cs
├── Extensions/              # Service configuration extensions
│   └── ServiceCollectionExtensions.cs
├── Middlewares/            # Custom middleware components
│   └── ExceptionHandlingMiddleware.cs
├── Models/                 # API-specific models and DTOs
│   ├── Auth/               # Auth request models
│   │   └── AuthRequests.cs
│   └── Common/             # Common API models
│       ├── ApiResponse.cs  # Standard response wrapper
│       ├── ClubRequest.cs  # Request models
│       └── UserRequest.cs  # User request models
├── Properties/             # Project properties and launch settings
├── appsettings*.json      # Configuration files for different environments
├── Program.cs             # Application entry point and configuration
├── Dockerfile            # Container configuration
└── WebAPI.csproj         # Project file with dependencies
```

### Folder Organization Principles
- **Controllers** - Organized by domain/entity (Common, Floorball, Hockey)
- **Models** - Request/response models specific to API layer
- **Extensions** - Service registration and configuration helpers
- **Middlewares** - Custom middleware for cross-cutting concerns

## 🚀 Core Features

### 1. **CRUD Operations for League Entities**
- Comprehensive REST endpoints for clubs, teams, players, matches, seasons
- Standardized HTTP verbs (GET, POST, PUT, DELETE)
- Consistent URL patterns and resource naming

### 2. **MediatR Integration**
- Complete CQRS implementation with command/query separation
- Decoupled controllers from business logic
- Pipeline behaviors for cross-cutting concerns

### 3. **Scalar API Documentation**
```csharp
// Modern, interactive documentation available at /scalar/v1
app.MapScalarApiReference(options =>
{
    options.WithTitle("MyLeague Club API Documentation")
           .WithTheme(ScalarTheme.Purple)
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});
```

### 4. **Global Exception Handling**
- Centralized error handling with ExceptionHandlingMiddleware
- Consistent error response format
- Proper HTTP status code mapping

### 5. **Request Validation**
- FluentValidation integration for automatic model validation
- Comprehensive validation error responses
- Custom validation rules support

### 6. **Structured Logging**
- Serilog integration with multiple output sinks
- Request/response logging with correlation IDs
- Performance monitoring and error tracking

### 7. **CORS Support**
- Flexible CORS configuration for different environments
- Support for development and production policies
- Pre-flight request handling

### 8. **Health Checks**
- Built-in health monitoring endpoint at `/health`
- Dependency health verification
- Load balancer integration support

## 🔌 API Endpoints

### Authentication (`/api/auth`)
| Method | Endpoint | Auth | Description | Response |
|--------|----------|------|-------------|----------|
| POST | `/api/auth/login` | No | Request login code (sent to email) | `ApiResponse` |
| POST | `/api/auth/verify` | No | Verify login code, get tokens | `ApiResponse<AuthTokenDto>` |
| POST | `/api/auth/refresh` | No | Refresh expired access token | `ApiResponse<AuthTokenDto>` |
| POST | `/api/auth/logout` | No | Revoke refresh token | `ApiResponse` |
| GET | `/api/auth/me` | Yes | Get current user info | `ApiResponse<UserDto>` |

**Authentication flow:**
1. `POST /api/auth/login` with `{ "email": "user@example.com" }` -- a 6-digit code is sent to the email (in development, the code is logged to the console)
2. `POST /api/auth/verify` with `{ "email": "user@example.com", "code": "123456" }` -- returns `{ accessToken, refreshToken, expiresAt }`
3. Include `Authorization: Bearer <accessToken>` header on protected endpoints
4. When the access token expires, call `POST /api/auth/refresh` with `{ "refreshToken": "..." }` to get a new token pair
5. To log out, call `POST /api/auth/logout` with `{ "refreshToken": "..." }`

**Default development user:** `test@myleague.local` (created automatically on first startup)

### Club Management
| Method | Endpoint | Description | Response |
|--------|----------|-------------|----------|
| GET | `/api/clubs` | Get all clubs | `ApiResponse<List<ClubDto>>` |
| GET | `/api/clubs/{id}` | Get club by ID | `ApiResponse<ClubDto>` |
| POST | `/api/clubs` | Create new club | `ApiResponse<ClubDto>` |
| PUT | `/api/clubs/{id}` | Update existing club | `ApiResponse<ClubDto>` |
| DELETE | `/api/clubs/{id}` | Delete club | `ApiResponse` |

### User Management (`/api/users`) -- Requires Authentication
| Method | Endpoint | Description | Response |
|--------|----------|-------------|----------|
| GET | `/api/users` | Get all users | `ApiResponse<List<UserDto>>` |
| GET | `/api/users/{id}` | Get user by ID | `ApiResponse<UserDto>` |
| GET | `/api/users/by-email?email=` | Get user by email | `ApiResponse<UserDto>` |
| GET | `/api/users/by-person/{personId}` | Get user by person ID | `ApiResponse<UserDto>` |
| POST | `/api/users` | Create new user | `ApiResponse<UserDto>` |
| PUT | `/api/users/{id}` | Update user | `ApiResponse<UserDto>` |
| DELETE | `/api/users/{id}` | Delete user | `ApiResponse` |

### System Endpoints
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Health check status |
| GET | `/scalar/v1` | Interactive API documentation |
| GET | `/swagger/v1/swagger.json` | OpenAPI specification |

## 📝 Request/Response Format

### Standard Success Response
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Manchester United",
    "city": "Manchester",
    "country": "England"
  },
  "message": "Operation completed successfully",
  "errors": null
}
```

### Standard Error Response
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "Name is required",
    "City must not exceed 100 characters"
  ]
}
```

### Pagination Response
```json
{
  "success": true,
  "data": {
    "items": [ /* array of items */ ],
    "totalCount": 150,
    "page": 1,
    "pageSize": 50,
    "totalPages": 3
  },
  "message": "Data retrieved successfully",
  "errors": null
}
```

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK or later
- PostgreSQL 13+ database server
- Visual Studio 2022 17.8+ or Visual Studio Code
- Docker (optional, for containerized deployment)

### Environment Setup

1. **Clone and Navigate**
   ```bash
   git clone <repository-url>
   cd src/backend/WebAPI
   ```

2. **Configure Database**
   Update `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=myleague;Username=postgres;Password=postgres;Port=5432"
     }
   }
   ```

3. **Install Dependencies**
   ```bash
   dotnet restore
   ```

4. **Run Database Migrations**
   ```bash
   # From the Infrastructure project directory
   cd ../Infrastructure
   dotnet ef database update
   cd ../WebAPI
   ```

5. **Start the Application**
   ```bash
   dotnet run
   # Or with specific environment
   dotnet run --environment Development
   ```

### Application URLs
- **API Base**: https://localhost:65532
- **API Documentation**: https://localhost:65532/scalar/v1
- **Health Check**: https://localhost:65532/health
- **OpenAPI Spec**: https://localhost:65532/swagger/v1/swagger.json

## 🔧 Configuration

### Environment-Specific Settings
- **Development**: `appsettings.Development.json` - Local development settings
- **Production**: `appsettings.Production.json` - Production optimizations
- **Docker**: `appsettings.Docker.json` - Container-specific configuration

### Key Configuration Sections
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=myleague;..."
  },
  "Jwt": {
    "Issuer": "MyLeague",
    "Audience": "MyLeague",
    "SecretKey": "<secret>",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "LoginCode": {
    "ExpirationMinutes": 10,
    "CodeLength": 6,
    "MaxAttempts": 5
  },
  "Seed": {
    "AdminEmail": ""
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/api-.log" } }
    ]
  },
  "AllowedHosts": "*"
}
```

## 📊 Logging & Monitoring

### Logging Configuration
- **Structured Logging** with Serilog for better queryability
- **Multiple Sinks** including Console, File, and Seq (when configured)
- **Request Correlation** with unique request IDs
- **Performance Tracking** with request duration logging

### Log Locations
- **Console Output** - Real-time development feedback
- **File Logs** - `logs/myleague-api-{date}.log`
- **Seq Dashboard** - `http://localhost:5341` (when configured)

### Health Monitoring
- **Health Check Endpoint** - `/health` for load balancer health checks
- **Dependency Verification** - Database and external service health
- **Custom Health Checks** - Extensible health check framework

## 🐳 Docker Support

### Dockerfile Configuration
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
# ... container configuration
```

### Docker Compose Integration
```yaml
services:
  webapi:
    build: ./WebAPI
    ports:
      - "65532:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker
```

## 🔒 Security & Authentication

### Passwordless Email Authentication
The API uses a passwordless authentication system. No passwords are stored in the database. Users log in with their email and a one-time code.

**How it works:**
1. User requests a login code by providing their email address
2. A cryptographically secure 6-digit code is generated and sent to the email
   - **Development**: Code is printed to the console (look for `[LOGIN CODE]` in output)
   - **Production**: Code is sent via Azure Communication Services Email
3. User submits the code; if valid, the API returns a JWT access token and refresh token
4. Protected endpoints require the `Authorization: Bearer <accessToken>` header
5. Tokens can be refreshed, and logout revokes the refresh token

**Security measures:**
- **Brute-force protection** -- After 5 failed code attempts, the code is locked; user must request a new one
- **Short-lived codes** -- Login codes expire after 10 minutes (configurable)
- **Refresh token rotation** -- Every refresh issues a new token and revokes the old; reusing a revoked token revokes all tokens for the user (theft detection)
- **Hashed storage** -- Only SHA256 hashes of refresh tokens are stored
- **Cryptographic generation** -- Codes and tokens are generated with `RandomNumberGenerator`

### Configuration
Authentication is configured in `appsettings.json` / `appsettings.Development.json`:

```json
{
  "Jwt": {
    "Issuer": "MyLeague",
    "Audience": "MyLeague",
    "SecretKey": "<your-secret-key>",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "LoginCode": {
    "ExpirationMinutes": 10,
    "CodeLength": 6,
    "MaxAttempts": 5
  },
  "AzureCommunicationServices": {
    "ConnectionString": "",
    "SenderAddress": ""
  },
  "Seed": {
    "AdminEmail": ""
  }
}
```

### Database Seeding
- **Development**: A default test user (`test@myleague.local`, Admin role) is automatically created on startup if it does not exist. Use this email to request a login code and find it in the console output.
- **Production**: Set `Seed__AdminEmail` as an environment variable to create an initial admin user on first deployment.

### Other Security Features
- **HTTPS Enforcement** - Automatic HTTPS redirection
- **CORS Configuration** - Flexible cross-origin policy management
- **Input Validation** - Comprehensive request validation with FluentValidation
- **Error Handling** - Secure error information disclosure

### Planned Enhancements
- **Rate Limiting** - API throttling and abuse prevention
- **API Versioning** - Backward compatibility management
- **Input Sanitization** - XSS and injection prevention

## 🔗 Integration Points

### Application Layer Integration
- **MediatR Commands** - Write operations through command handlers
- **MediatR Queries** - Read operations through query handlers
- **Result Pattern** - Consistent success/failure handling

### Infrastructure Layer Integration
- **Dependency Injection** - Service registration from Infrastructure layer
- **Database Context** - Entity Framework Core integration
- **External Services** - Third-party API integrations

## 📚 Documentation Resources

### API Documentation
- **Interactive Docs** - Scalar UI at `/scalar/v1` (development)
- **OpenAPI Specification** - Machine-readable API definition
- **XML Comments** - IntelliSense and documentation generation

### Development Guides
- **Controller Development** - Best practices for API endpoint creation
- **Middleware Development** - Custom middleware implementation
- **Testing Strategies** - Unit and integration testing approaches

For detailed development guidance, see [WebAPIDevelopmentGuide.md](./WebAPIDevelopmentGuide.md)

## 🤝 Contributing

### Development Standards
- Follow REST API conventions and HTTP status code guidelines
- Implement comprehensive input validation and error handling
- Use standard response formats for consistency
- Document all endpoints with XML comments
- Write unit and integration tests for all controllers

### Code Quality
- Maintain test coverage above 80%
- Follow C# coding conventions and naming standards
- Use async/await for all I/O operations
- Implement proper logging and error handling
- Follow SOLID principles and clean code practices 