# MyLeague Backend - Docker Setup

This directory contains the Docker configuration for the MyLeague backend API with PostgreSQL database and log visualization.

## Architecture

The Docker setup includes:

- **MyLeague API**: .NET 9 Web API application
- **PostgreSQL**: Database server (v16)
- **Seq**: Log visualization and analysis platform (Serilog-compatible)

## Quick Start

### Prerequisites

- Docker Desktop or Docker Engine
- Docker Compose

### Running the Application

1. **Navigate to the backend directory:**
   ```bash
   cd src/backend
   ```

2. **Start all services:**
   ```bash
   docker-compose up -d
   ```

3. **View logs (optional):**
   ```bash
   docker-compose logs -f api
   ```

### Accessing the Services

| Service | URL | Description |
|---------|-----|-------------|
| **API** | http://localhost:8080 | MyLeague API with Swagger UI |
| **API Health Check** | http://localhost:8080/health | Health check endpoint |
| **Seq (Logs)** | http://localhost:5341 | Log visualization dashboard |
| **PostgreSQL** | localhost:5432 | Database (use your preferred client) |

### Database Connection

```
Host: localhost
Port: 5432
Database: myleague
Username: postgres
Password: postgres
```

## Log Visualization with Seq

Seq is a powerful log analysis platform that integrates seamlessly with Serilog. Your application logs will automatically appear in Seq at http://localhost:5341.

### Features:
- **Real-time log viewing**: See logs as they happen
- **Structured logging**: Filter and search by log properties
- **Log levels**: Filter by Information, Warning, Error, etc.
- **Time-based analysis**: View logs over time periods
- **Search and filtering**: Advanced query capabilities

### Accessing Logs:
1. Open http://localhost:5341 in your browser
2. You'll see all logs from your MyLeague API
3. Use the search box to filter logs (e.g., `@Level = 'Error'`)
4. Click on any log entry to see detailed information

## Docker Commands

### Start services
```bash
docker-compose up -d
```

### Stop services
```bash
docker-compose down
```

### View logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f postgres
docker-compose logs -f seq
```

### Rebuild and restart
```bash
docker-compose down
docker-compose up --build -d
```

### Reset everything (including data)
```bash
docker-compose down -v
docker-compose up -d
```

## Authentication

The application uses **passwordless email authentication**. No passwords are stored in the database.

### How It Works
1. Request a login code by calling `POST /api/auth/login` with `{ "email": "user@example.com" }`
2. **In Docker/Development**: The login code is printed to the API container's console output. View it with:
   ```bash
   docker-compose logs -f api
   ```
   Look for a log line containing `[LOGIN CODE]`.
3. Verify the code by calling `POST /api/auth/verify` with `{ "email": "...", "code": "123456" }` to receive a JWT access token and refresh token.
4. Use the access token as `Authorization: Bearer <token>` on protected endpoints.

### Default Test User
In development, a test user (`test@myleague.local`) is automatically created on first startup. Use this email to test the login flow.

### Admin User for Production
To create an initial admin user in production, set the `Seed__AdminEmail` environment variable in `docker-compose.yml`:

```yaml
services:
  webapi:
    environment:
      - Seed__AdminEmail=admin@yourdomain.com
```

### JWT & Auth Environment Variables
| Variable | Description | Default |
|----------|-------------|---------|
| `Jwt__SecretKey` | Secret key for signing JWTs | *(set in appsettings)* |
| `Jwt__AccessTokenExpirationMinutes` | Access token lifetime | `15` |
| `Jwt__RefreshTokenExpirationDays` | Refresh token lifetime | `7` |
| `LoginCode__ExpirationMinutes` | Login code validity | `10` |
| `LoginCode__MaxAttempts` | Max failed code attempts before lockout | `5` |
| `Seed__AdminEmail` | Email for initial admin user (optional) | *(empty)* |
| `AzureCommunicationServices__ConnectionString` | Azure Email connection string (production) | *(empty)* |
| `AzureCommunicationServices__SenderAddress` | Sender email address (production) | *(empty)* |

## Development

### File Structure
```
src/backend/
├── Dockerfile                 # Multi-stage build for the API
├── docker-compose.yml         # Complete stack definition
├── .dockerignore             # Files to exclude from build
├── appsettings.Production.json # Production config with Seq
└── WebAPI/
    ├── appsettings.json       # Development config
    └── ...
```

### Environment Configuration

The application uses different configuration based on the environment:

- **Development**: Uses `appsettings.json` + `appsettings.Development.json`
- **Production** (Docker): Uses `appsettings.json` + `appsettings.Production.json`

### Adding Database Migrations

If you need to run Entity Framework migrations:

```bash
# Enter the API container
docker-compose exec api bash

# Run migrations (if you have them set up)
dotnet ef database update
```

## Troubleshooting

### API Not Starting
1. Check if PostgreSQL is healthy:
   ```bash
   docker-compose ps
   ```
2. View API logs:
   ```bash
   docker-compose logs api
   ```

### Database Connection Issues
1. Ensure PostgreSQL container is running:
   ```bash
   docker-compose ps postgres
   ```
2. Check database logs:
   ```bash
   docker-compose logs postgres
   ```

### Seq Not Showing Logs
1. Verify Seq is running: http://localhost:5341
2. Check API configuration for Seq sink
3. Restart the API container:
   ```bash
   docker-compose restart api
   ```

### Port Conflicts
If you get port conflicts, you can modify the ports in `docker-compose.yml`:

```yaml
ports:
  - "8081:8080"  # Change 8080 to 8081 if port 8080 is taken
```

## Data Persistence

- **PostgreSQL data**: Stored in Docker volume `postgres_data`
- **Seq data**: Stored in Docker volume `seq_data`
- **Application logs**: Mapped to `./logs` directory

To completely reset data, remove the volumes:
```bash
docker-compose down -v
```

## Production Considerations

For production deployment, consider:

1. **Environment Variables**: Use Docker secrets or environment files
2. **SSL/TLS**: Configure HTTPS certificates
3. **Resource Limits**: Set memory and CPU limits in docker-compose.yml
4. **Backup Strategy**: Regular PostgreSQL backups
5. **Log Retention**: Configure Seq retention policies
6. **Monitoring**: Add health checks and monitoring tools 