# MyLeague Club API

## Overview
ASP.NET Core 9.0 Web API for managing football clubs in the MyLeague application. This API follows Clean Architecture principles and uses MediatR for CQRS pattern implementation.

## Features
- ✅ **CRUD Operations** for Clubs
- ✅ **MediatR Integration** for commands and queries
- ✅ **Scalar API Documentation** with modern OpenAPI interface
- ✅ **Global Exception Handling** with structured error responses
- ✅ **Request Validation** with FluentValidation
- ✅ **Structured Logging** with Serilog
- ✅ **CORS Support** for cross-origin requests
- ✅ **Health Checks** endpoint

## API Documentation

This API uses **Scalar** for modern, interactive API documentation. Scalar provides a beautiful and user-friendly interface for exploring and testing the API.

### Accessing Documentation
- **Scalar UI**: `/scalar/v1` (development environment)
- **OpenAPI JSON**: `/swagger/v1/swagger.json`

### Features of Scalar Documentation
- 🎨 **Modern UI** with beautiful design and dark/light themes
- 🔍 **Interactive Testing** - test endpoints directly from the documentation
- 📝 **Code Examples** - automatic generation of HTTP client code in multiple languages
- 🔐 **Authentication Support** - built-in support for Bearer tokens and other auth methods
- 📱 **Responsive Design** - works perfectly on desktop and mobile devices

### Updating Documentation
The API documentation is automatically generated from:
- Controller actions and their attributes
- XML documentation comments (when `GenerateDocumentationFile` is enabled)
- Data annotations on models
- OpenAPI configuration in `ServiceCollectionExtensions.cs`

To enhance documentation:
1. Add XML comments to your controllers and models
2. Use data annotations for validation and documentation
3. Update the OpenAPI configuration in `Extensions/ServiceCollectionExtensions.cs`

## API Endpoints

### Club Management
```
GET    /api/clubs              # Get all clubs
GET    /api/clubs/{id}         # Get club by ID
POST   /api/clubs              # Create new club
PUT    /api/clubs/{id}         # Update existing club
DELETE /api/clubs/{id}         # Delete club
```

### System
```
GET    /health                 # Health check endpoint
GET    /scalar/v1              # API documentation (dev environment)
GET    /swagger/v1/swagger.json # OpenAPI JSON specification
```

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- PostgreSQL database
- Visual Studio 2022 or VS Code

### Configuration
Update `appsettings.json` or `appsettings.Development.json` with your database connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=myleague;Username=postgres;Password=postgres"
  }
}
```

### Running the API
```bash
# Navigate to the WebAPI directory
cd src/backend/WebAPI

# Restore packages
dotnet restore

# Run the application
dotnet run

# Or run in development mode
dotnet run --environment Development
```

The API will be available at:
- **HTTPS**: https://localhost:65532
- **HTTP**: http://localhost:65533
- **API Documentation**: https://localhost:65532/scalar/v1 (development only)

## Request/Response Format

### Standard Response Format
All API responses follow a consistent format:

```json
{
  "success": true,
  "data": { /* response data */ },
  "message": "Operation completed successfully",
  "errors": []
}
```

### Error Response Format
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": ["Error message"]
}
```

### Example: Create Club Request
```json
{
  "name": "Manchester United",
  "city": "Manchester",
  "country": "England",
  "foundingDate": "1878-01-01T00:00:00Z",
  "websiteUrl": "https://www.manutd.com",
  "logoUrl": "https://www.manutd.com/logo.png",
  "contactEmail": "contact@manutd.com"
}
```

## Logging
Logs are written to:
- **Console** (structured format)
- **Files** (`logs/myleague-api-{date}.log`)

Log levels can be configured in `appsettings.json` under the `Serilog` section.

## Architecture
This API layer integrates with:
- **Application Layer**: Contains business logic, commands, and queries
- **Infrastructure Layer**: Data access and external services
- **Domain Layer**: Core business entities and rules

The API uses MediatR to send commands and queries to the Application layer, maintaining clean separation of concerns. 