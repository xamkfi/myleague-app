# MyLeague Backend - Visual Studio Docker Setup

This guide explains how to run and debug your MyLeague backend using Docker directly from Visual Studio.

## Prerequisites

1. **Visual Studio 2022** (Community, Professional, or Enterprise)
2. **Docker Desktop** installed and running
3. **Container Development Tools** workload in Visual Studio

### Installing Container Development Tools

If you haven't installed the Container Development Tools:

1. Open **Visual Studio Installer**
2. Click **Modify** for your Visual Studio installation
3. Go to **Workloads** tab
4. Check **ASP.NET and web development**
5. In the **Installation details**, ensure **Container development tools** is checked
6. Click **Modify**

## Getting Started

### Option 1: Using Docker Compose (Recommended)

1. **Open the Solution**
   - Open `MyLeague.sln` in Visual Studio
   - You should see a `docker-compose` project in Solution Explorer

2. **Set docker-compose as Startup Project**
   - Right-click on `docker-compose` project in Solution Explorer
   - Select "Set as Startup Project"

3. **Run with Docker Compose**
   - Press `F5` or click the green "Docker Compose" button in the toolbar
   - Visual Studio will build and start all containers (API, PostgreSQL, Seq)
   - A browser will automatically open to your API

### Option 2: Using Launch Profile

1. **Select Launch Profile**
   - In the debug dropdown (next to the green play button)
   - Select "Docker Compose" from the dropdown

2. **Start Debugging**
   - Press `F5` or click the green play button
   - Visual Studio will start all containers and attach the debugger

## What Happens When You Run

When you start with Docker Compose, Visual Studio will:

1. **Build the Docker images** for your application
2. **Start PostgreSQL container** (database)
3. **Start Seq container** (log visualization)
4. **Start API container** (your application)
5. **Attach the debugger** (if debugging)
6. **Open browser** to http://localhost:8080

## Available Services

| Service | URL | Description |
|---------|-----|-------------|
| **API (Swagger)** | http://localhost:8080 | Your API with interactive documentation |
| **Health Check** | http://localhost:8080/health | API health status |
| **Seq (Logs)** | http://localhost:5341 | Real-time log viewer |
| **PostgreSQL** | localhost:5432 | Database (postgres/postgres) |

## Debugging Features

### Breakpoints
- Set breakpoints in your C# code as usual
- They will work when running in Docker containers
- Visual Studio automatically configures the debugging connection

### Hot Reload
- Edit your code while debugging
- Visual Studio will apply changes without restarting containers
- Works with most code changes

### Log Viewing
- **Visual Studio Output Window**: Shows container startup logs
- **Seq Dashboard**: http://localhost:5341 for structured log analysis
- **Docker Desktop**: View all container logs

## Development Workflow

### Making Code Changes
1. Edit your C# code in Visual Studio
2. Save files (Ctrl+S)
3. Visual Studio automatically rebuilds and updates containers
4. Refresh browser to see changes

### Database Changes
1. If you need to reset the database:
   ```
   Right-click docker-compose → Open in Terminal
   docker-compose down -v
   docker-compose up -d
   ```

### Viewing Logs in Seq
1. Open http://localhost:5341
2. See all application logs in real-time
3. Filter by log level, source, or custom queries
4. Click any log entry for detailed information

## Troubleshooting

### "Docker is not running" Error
1. Make sure Docker Desktop is running
2. Check Docker Desktop system tray icon
3. Restart Docker Desktop if needed

### "Port already in use" Error
1. Stop other applications using ports 8080, 5432, or 5341
2. Or modify ports in `docker-compose.override.yml`

### Containers Not Starting
1. Check Docker Desktop for error messages
2. View Output Window → Show output from: "Container Tools"
3. Clean and rebuild solution (Build → Clean Solution → Rebuild Solution)

### Debugging Not Working
1. Ensure "Docker Compose" is selected as startup project
2. Make sure you're using F5 (not Ctrl+F5)
3. Check that breakpoints are showing as solid red circles

### Database Connection Issues
1. Wait for PostgreSQL to fully start (can take 30-60 seconds first time)
2. Check Container Tools output for PostgreSQL status
3. Verify connection string in appsettings.Development.json

## Advanced Configuration

### Customizing Environment
Edit `docker-compose.override.yml` to:
- Change port mappings
- Add environment variables
- Configure volume mounts
- Adjust service dependencies

### Production vs Development
- **Development**: Uses `docker-compose.override.yml` (HTTP, debugging enabled)
- **Production**: Uses base `docker-compose.yml` (HTTPS, optimized)

### Managing Container Data
```powershell
# View running containers
docker-compose ps

# View logs
docker-compose logs api
docker-compose logs postgres
docker-compose logs seq

# Restart specific service
docker-compose restart api

# Reset all data (including database)
docker-compose down -v
docker-compose up -d
```

## Performance Tips

1. **Use WSL 2** backend in Docker Desktop (Windows)
2. **Allocate sufficient memory** to Docker (4GB+ recommended)
3. **Close unnecessary applications** when developing
4. **Use .dockerignore** to exclude unnecessary files from builds

## Visual Studio Container Tools Features

### Container Window
- View → Other Windows → Containers
- See all running containers
- Browse file systems
- View environment variables
- Access container terminals

### Docker Files View
- Automatically generated Dockerfile integration
- Build context and layer inspection
- Image management

### Publishing
- Right-click WebAPI project → Publish
- Choose "Container Registry" for cloud deployment
- Visual Studio can push to Azure Container Registry, Docker Hub, etc.

## Next Steps

- **Add Entity Framework Migrations**: Run migrations in container environment
- **Configure HTTPS**: Set up SSL certificates for production
- **CI/CD Integration**: Use the Docker setup in Azure DevOps or GitHub Actions
- **Monitoring**: Add Application Insights or other monitoring tools 