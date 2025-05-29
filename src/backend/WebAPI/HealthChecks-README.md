# MyLeague Health Checks Documentation

## Overview

The MyLeague API implements comprehensive health checks following Clean Architecture principles and Microsoft best practices. The health check system monitors various aspects of the application including database connectivity, system resources, and application services.

## Health Check Endpoints

### 1. Detailed Health Check
- **URL**: `/health`
- **Method**: GET
- **Description**: Returns detailed health information for all registered health checks
- **Response Format**: JSON with detailed status, duration, and data for each check

```json
{
  "status": "Unhealthy",
  "duration": 15078.2765,
  "checkedAt": "2025-05-29T17:41:20.4231028Z",
  "checks": [
    {
      "name": "self",
      "status": "Healthy",
      "description": "API is running",
      "duration": 0.062,
      "data": {},
      "tags": []
    },
    {
      "name": "postgresql-connection",
      "status": "Unhealthy",
      "description": "Name or service not known",
      "duration": 7856.8907,
      "data": {},
      "tags": ["database", "postgresql"]
    }
  ]
}
```

### 2. Readiness Check
- **URL**: `/health/ready`
- **Method**: GET
- **Description**: Simple endpoint for load balancers and orchestrators
- **Response**: "Healthy" or "Unhealthy" (text/plain)

### 3. Liveness Check
- **URL**: `/health/live`
- **Method**: GET
- **Description**: Basic liveness probe
- **Response**: "Alive" (text/plain)

### 4. Health Check Controller
- **Base URL**: `/api/health`
- **Methods**: 
  - `GET /api/health` - Detailed health status
  - `GET /api/health/tag/{tag}` - Health status filtered by tag
  - `GET /api/health/ready` - Readiness check
  - `GET /api/health/live` - Liveness check

### 5. Health Check UI
- **URL**: `/health-ui`
- **Description**: Web-based dashboard for monitoring health checks
- **Features**: Real-time monitoring, historical data, visual status indicators

### 6. Custom Health Dashboard
- **URL**: `/health-test.html`
- **Description**: Enhanced custom dashboard with detailed health check visualization
- **Features**: 
  - Beautiful, modern UI with color-coded status indicators
  - Detailed information for each health check including descriptions, durations, and data
  - Tag-based categorization
  - Auto-refresh every 30 seconds
  - Multi-endpoint support with automatic fallback
  - Enhanced error handling and troubleshooting information

## Access URLs

### Development (Visual Studio / dotnet run)
- **Health Check UI**: `http://localhost:65533/health-ui`
- **Custom Health Dashboard**: `http://localhost:65533/health-test.html`
- **Direct Health Endpoint**: `http://localhost:65533/health`
- **API Documentation**: `http://localhost:65533/scalar/v1`
- **HTTPS versions**: Replace `65533` with `65532`

### Docker Environment
- **Health Check UI**: `http://localhost:8080/health-ui`
- **Custom Health Dashboard**: `http://localhost:8080/health-test.html`
- **Direct Health Endpoint**: `http://localhost:8080/health`
- **API Documentation**: `http://localhost:8080/scalar/v1`

## Implemented Health Checks

### 1. Self Check
- **Name**: `self`
- **Description**: Basic API availability check
- **Tags**: None
- **Expected Status**: Always Healthy when API is running

### 2. PostgreSQL Connection
- **Name**: `postgresql-connection`
- **Description**: Tests raw PostgreSQL database connectivity
- **Tags**: `database`, `postgresql`
- **Common Issues**: "Name or service not known" when PostgreSQL is not running

### 3. Common Database Context
- **Name**: `common-database`
- **Description**: Entity Framework Core health check for CommonDbContext
- **Tags**: `database`, `ef-core`, `common`
- **Dependencies**: Requires PostgreSQL connection

### 4. Floorball Database Context
- **Name**: `floorball-database`
- **Description**: Entity Framework Core health check for FloorballDbContext
- **Tags**: `database`, `ef-core`, `floorball`
- **Dependencies**: Requires PostgreSQL connection

### 5. Database Operations
- **Name**: `database-operations`
- **Description**: Custom health check that performs actual database queries
- **Tags**: `database`, `custom`
- **Checks**: 
  - Database connectivity for both contexts
  - Basic query operations (count queries)
  - Returns entity counts as additional data
- **Common Issues**: "Common database is not accessible" when PostgreSQL is down

### 6. Application Services
- **Name**: `application-services`
- **Description**: Verifies critical application services are registered and accessible
- **Tags**: `services`, `dependencies`
- **Checks**:
  - Repository services (Club, Person, Player, Team, Match, Season)
  - Unit of Work service
  - Service resolution and instantiation
- **Data Returned**:
  - ServicesChecked: Number of services verified
  - ServicesHealthy: Number of healthy services
  - Individual service status for each repository

### 7. Disk Storage
- **Name**: `disk-storage`
- **Description**: Monitors available disk space
- **Tags**: `system`, `storage`
- **Threshold**: 1000 MB minimum free space
- **Platform**: Checks C:\ on Windows, / on Linux/Docker

### 8. Memory Usage
- **Name**: `memory-usage`
- **Description**: Monitors process allocated memory
- **Tags**: `system`, `memory`
- **Threshold**: 1000 MB maximum allocated memory
- **Data**: Shows allocated megabytes in description

### 9. Private Memory
- **Name**: `private-memory`
- **Description**: Monitors private memory usage
- **Tags**: `system`, `memory`
- **Threshold**: 1.5 GB maximum private memory

## Health Check Tags

Health checks are organized using tags for easy filtering:

- **`database`**: All database-related checks
- **`postgresql`**: PostgreSQL-specific checks
- **`ef-core`**: Entity Framework Core checks
- **`common`**: Common database context checks
- **`floorball`**: Floorball database context checks
- **`custom`**: Custom implementation checks
- **`services`**: Application service checks
- **`dependencies`**: Dependency injection checks
- **`system`**: System resource checks
- **`storage`**: Storage-related checks
- **`memory`**: Memory-related checks

## Configuration

Health checks are configured in `appsettings.json` with environment-specific overrides:

### Base Configuration (appsettings.json)
```json
{
  "HealthChecks": {
    "UI": {
      "EvaluationTimeInSeconds": 30,
      "MaximumHistoryEntriesPerEndpoint": 50,
      "Endpoint": "http://localhost:65533/health"
    },
    "Database": {
      "TimeoutSeconds": 30
    },
    "System": {
      "DiskStorage": {
        "MinimumFreeMegabytes": 1000
      },
      "Memory": {
        "MaximumMegabytesAllocated": 1000,
        "MaximumPrivateMemoryBytes": 1500000000
      }
    }
  }
}
```

### Environment-Specific Endpoints
- **Development**: `http://localhost:65533/health`
- **Docker**: `http://webapi:8080/health`
- **Production**: `http://localhost:65533/health` (adjust as needed)

## Architecture

The health check implementation follows Clean Architecture principles:

### Infrastructure Layer
- **Location**: `src/backend/Infrastructure/HealthChecks/`
- **Components**:
  - `DatabaseHealthCheck.cs` - Custom database operations check
  - `ApplicationServicesHealthCheck.cs` - Service registration verification
  - `HealthCheckExtensions.cs` - Registration extension methods

### WebAPI Layer
- **Location**: `src/backend/WebAPI/Controllers/`
- **Components**:
  - `HealthController.cs` - RESTful health check endpoints
  - `Program.cs` - Health check middleware configuration
  - `wwwroot/health-test.html` - Custom health dashboard

### Configuration
- **Location**: `src/backend/WebAPI/DependencyInjections/`
- **Components**:
  - `ServiceCollectionExtensions.cs` - Health Check UI configuration with environment-specific endpoints

## Usage Examples

### Checking Overall Health (Development)
```bash
curl -X GET "http://localhost:65533/health" -H "accept: application/json"
```

### Checking Overall Health (Docker)
```bash
curl -X GET "http://localhost:8080/health" -H "accept: application/json"
```

### Checking Database Health Only
```bash
curl -X GET "http://localhost:65533/api/health/tag/database" -H "accept: application/json"
```

### Simple Readiness Check
```bash
curl -X GET "http://localhost:65533/health/ready"
```

### Monitoring with Health Check UI
- **Development**: Navigate to `http://localhost:65533/health-ui`
- **Docker**: Navigate to `http://localhost:8080/health-ui`

### Custom Health Dashboard
- **Development**: Navigate to `http://localhost:65533/health-test.html`
- **Docker**: Navigate to `http://localhost:8080/health-test.html`

## Monitoring and Alerting

### Kubernetes/Docker
Use the readiness and liveness endpoints for container orchestration:

```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 10

readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 5
  periodSeconds: 5
```

### Load Balancers
Configure load balancers to use `/health/ready` for health checks.

### Monitoring Tools
- Use `/health` endpoint for detailed monitoring
- Parse JSON response for specific check statuses
- Set up alerts based on health check status changes
- Use the custom dashboard for visual monitoring

## Troubleshooting

### Common Issues

1. **Database Connection Failures**
   - **Error**: "Name or service not known" or "Connection refused"
   - **Solution**: 
     - Check connection string in appsettings
     - Verify PostgreSQL server is running
     - Check network connectivity
     - For local development, ensure PostgreSQL is accessible on the configured host

2. **Health Check UI Not Showing Details**
   - **Error**: UI shows basic status but no individual check details
   - **Solution**: 
     - Verify the Health Check UI endpoint configuration matches the running application port
     - Check that the application is running on the expected port
     - Review browser console for CORS or network errors

3. **Service Registration Issues**
   - **Error**: Application services health check fails
   - **Solution**:
     - Verify dependency injection configuration
     - Check for circular dependencies
     - Review service lifetimes

4. **Memory/Disk Warnings**
   - **Solution**:
     - Adjust thresholds in configuration
     - Monitor resource usage trends
     - Consider scaling or optimization

5. **Static Files Not Served**
   - **Error**: Custom health dashboard (health-test.html) returns 404
   - **Solution**: Ensure `app.UseStaticFiles()` is configured in Program.cs

### Debugging

Enable detailed logging for health checks by setting log level to Debug:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "MyLeague.Infrastructure.HealthChecks": "Debug",
        "HealthChecks.UI.Core": "Debug"
      }
    }
  }
}
```

### Health Check UI Configuration Issues

If the Health Check UI is not displaying individual check details:

1. **Check the endpoint configuration** in appsettings files
2. **Verify the application is running** on the configured port
3. **Check browser developer tools** for network errors
4. **Review application logs** for Health Check UI errors

## Best Practices

1. **Regular Monitoring**: Check health endpoints regularly
2. **Threshold Tuning**: Adjust resource thresholds based on environment
3. **Alerting**: Set up alerts for health check failures
4. **Documentation**: Keep health check documentation updated
5. **Testing**: Include health checks in integration tests
6. **Environment Configuration**: Use environment-specific health check endpoints
7. **Custom Dashboards**: Utilize the custom health dashboard for enhanced monitoring

## Security Considerations

- Health check endpoints expose system information
- Consider authentication for detailed health endpoints in production
- Use simple endpoints (`/health/ready`, `/health/live`) for external monitoring
- Limit detailed information exposure in production environments
- The custom health dashboard provides detailed system information - secure appropriately

## Recent Updates

### Health Check UI Improvements
- **Fixed endpoint configuration**: Now uses environment-specific endpoints
- **Enhanced error handling**: Better error messages when health checks fail
- **Custom dashboard**: Added beautiful, detailed health check visualization
- **Multi-endpoint support**: Custom dashboard tries multiple endpoints automatically
- **Static file serving**: Enabled serving of custom health dashboard

### Configuration Enhancements
- **Environment-specific endpoints**: Different ports for development vs Docker
- **Configurable health check endpoints**: No longer hardcoded
- **Improved error reporting**: Better visibility into health check failures 