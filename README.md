# MyLeague

## Docker Support for Visual Studio

This project has been configured to use Docker with Visual Studio. The following services are included:

- **MyLeague.Api** - ASP.NET Core Web API
- **PostgreSQL** - Database
- **Redis** - Caching
- **Adminer** - Database management tool

### Running from Visual Studio

1. Open the solution in Visual Studio
2. Right-click on the Docker Compose project and select "Set as Startup Project"
3. Press F5 or click the "Docker Compose" button in the toolbar to build and run the containers
4. Visual Studio will automatically open your browser to the Swagger UI

### Manual Docker Commands

If you want to run the Docker containers outside Visual Studio:

```bash
docker-compose up -d
```

### Accessing Services

- API: http://localhost:80 or https://localhost:443
- Swagger UI: http://localhost/swagger
- Adminer (database management): http://localhost:8080
  - System: PostgreSQL
  - Server: db
  - Username: postgres
  - Password: postgres
  - Database: myleague

### Stopping the Application

From Visual Studio:
- Click the Stop button in the toolbar

Using Docker Compose:
```bash
docker-compose down
```

To remove volumes and delete all data:
```bash
docker-compose down -v
```