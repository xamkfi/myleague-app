# MyLeague Health Checks Documentation

## Overview

The MyLeague API implements comprehensive health checks following Clean Architecture principles and Microsoft best practices. The health check system monitors various aspects of the application including database connectivity, system resources, and application services.

## Health Check Endpoints

### 1. Detailed Health Check
- **URL**: `/health`
- **Method**: GET
- **Description**: Returns every check, including memory, disk, row counts, and service resolution. A failing diagnostic check makes this endpoint non-200. It does not remove the App Service instance from traffic.
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
- **Description**: Checks tagged `ready` only: raw PostgreSQL plus `CommonDbContext`, `FloorballDbContext`, `FootballDbContext`, and `HockeyDbContext`
- **Used by**: App Service `healthCheckPath`, the production availability test, and deploy smoke tests
- **Response**: "Healthy" or "Unhealthy" (text/plain). HTTP 503 when any ready check fails

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

### 5. Health dashboard
- **URL**: `/health-test.html` (static file in `wwwroot`)
- **Alias**: `/health-ui` redirects to `/health-test.html`
- The Xabaril HealthChecks.UI package is not referenced. There is no history store or separate UI host.
- The page shows the current `/health` JSON: status, duration, description, and tags, and refreshes on a timer.

## Access URLs

### Development (Visual Studio / dotnet run)
- **Dashboard**: `http://localhost:65533/health-test.html` (`/health-ui` redirects here)
- **Direct Health Endpoint**: `http://localhost:65533/health`
- **API Documentation**: `http://localhost:65533/scalar/v1`
- **HTTPS versions**: Replace `65533` with `65532`

### Docker Environment
- **Dashboard**: `http://localhost:8080/health-test.html` (`/health-ui` redirects here)
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
- **Tags**: `database`, `postgresql`, `ready`
- **Common Issues**: "Name or service not known" when PostgreSQL is not running

### 3. Common Database Context
- **Name**: `common-database`
- **Description**: Entity Framework Core health check for CommonDbContext
- **Tags**: `database`, `ef-core`, `common`, `ready`
- **Dependencies**: Requires PostgreSQL connection

### 4. Floorball Database Context
- **Name**: `floorball-database`
- **Description**: Entity Framework Core health check for FloorballDbContext
- **Tags**: `database`, `ef-core`, `floorball`, `ready`
- **Dependencies**: Requires PostgreSQL connection

### 5. Football Database Context
- **Name**: `football-database`
- **Description**: Entity Framework Core health check for FootballDbContext
- **Tags**: `database`, `ef-core`, `football`, `ready`
- **Dependencies**: Requires PostgreSQL connection

### 6. Hockey Database Context
- **Name**: `hockey-database`
- **Description**: Entity Framework Core health check for HockeyDbContext
- **Tags**: `database`, `ef-core`, `hockey`, `ready`
- **Dependencies**: Requires PostgreSQL connection

### 7. Database Operations
- **Name**: `database-operations`
- **Description**: Diagnostic count queries. Not included in `/health/ready`
- **Tags**: `database`, `custom`
- **Checks**:
  - Connectivity for Common, Floorball, Football, and Hockey contexts
  - Club count, floorball player count, football team count, hockey team count
- **Common Issues**: "Common database is not accessible" when PostgreSQL is down

### 8. Application Services
- **Name**: `application-services`
- **Description**: Diagnostic service resolution. Not included in `/health/ready`
- **Tags**: `services`, `dependencies`
- **Checks**:
  - `IClubRepository`, `IPersonRepository`
  - Floorball player, team, match, and competition repositories
  - `IUnitOfWork`
  - Football and hockey repositories are not part of this check
  - Service resolution and instantiation
- **Data Returned**:
  - ServicesChecked: Number of services verified
  - ServicesHealthy: Number of healthy services
  - Individual service status for each repository

### 9. Disk Storage
- **Name**: `disk-storage`
- **Description**: Diagnostic disk space check. Not included in `/health/ready`
- **Tags**: `system`, `storage`
- **Threshold**: 1000 MB minimum free space
- **Platform**: Checks C:\ on Windows, / on Linux/Docker

### 10. Memory Usage
- **Name**: `memory-usage`
- **Description**: Diagnostic process-memory check. Not included in `/health/ready`
- **Tags**: `system`, `memory`
- **Threshold**: 1000 MB maximum allocated memory
- **Data**: Shows allocated megabytes in description

### 11. Private Memory
- **Name**: `private-memory`
- **Description**: Diagnostic private-memory check. Not included in `/health/ready`
- **Tags**: `system`, `memory`
- **Threshold**: 1.5 GB maximum private memory

## Health Check Tags

Health checks are organized using tags for easy filtering:

- **`database`**: All database-related checks
- **`postgresql`**: PostgreSQL-specific checks
- **`ef-core`**: Entity Framework Core checks
- **`common`**: Common database context checks
- **`floorball`**: Floorball database context checks
- **`football`**: Football database context checks
- **`hockey`**: Hockey database context checks
- **`ready`**: Checks that `/health/ready` and `/api/health/ready` run
- **`custom`**: Custom implementation checks
- **`services`**: Application service checks
- **`dependencies`**: Dependency injection checks
- **`system`**: System resource checks
- **`storage`**: Storage-related checks
- **`memory`**: Memory-related checks

## Configuration

Thresholds are hardcoded in `Infrastructure/HealthChecks/HealthCheckExtensions.cs` (disk 1000 MB free, process memory 1000 MB, private memory 1.5 GB). `appsettings.json` has a `HealthChecks` section, but nothing reads it.

Endpoints:

- **Development**: `http://localhost:65533/health`
- **Docker**: `http://localhost:8080/health`

## Architecture

### Infrastructure Layer
- **Location**: `src/backend/Infrastructure/HealthChecks/`
- **Components**:
  - `DatabaseHealthCheck.cs` — connectivity and count queries
  - `ApplicationServicesHealthCheck.cs` — service resolution
  - `HealthCheckExtensions.cs` — registration (`AddMyLeagueHealthChecks`)

### WebAPI Layer
- `Controllers/Health/HealthController.cs` — `/api/health`
- `Program.cs` — `/health`, `/health/ready`, `/health/live`, and the `/health-ui` redirect
- `wwwroot/health-test.html` — static dashboard

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

### Dashboard
- **Development**: `http://localhost:65533/health-test.html` (`/health-ui` redirects here)
- **Docker**: `http://localhost:8080/health-test.html`

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

2. **Dashboard shows no checks**
   - Open `/health` directly and confirm it returns JSON
   - Confirm `app.UseStaticFiles()` runs so `/health-test.html` is served

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
        "MyLeague.Infrastructure.HealthChecks": "Debug"
      }
    }
  }
}
```

## Best Practices

1. **Regular Monitoring**: Check health endpoints regularly
2. **Threshold Tuning**: Adjust resource thresholds based on environment
3. **Alerting**: Set up alerts for health check failures
4. **Documentation**: Keep health check documentation updated
5. **Testing**: Include health checks in integration tests
6. **Dashboard**: Use `/health-test.html` for a visual check; use `/health/ready` and `/health/live` for probes

## Security Considerations

- Health check endpoints expose system information
- Consider authentication for detailed health endpoints in production
- Use simple endpoints (`/health/ready`, `/health/live`) for external monitoring
- Limit detailed information exposure in production environments
- The custom health dashboard provides detailed system information - secure appropriately

## Gaps

- `application-services` resolves common and floorball repositories only.
- Disk and memory thresholds are hardcoded; the `HealthChecks` appsettings section is unused. 