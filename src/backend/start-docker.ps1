# MyLeague Backend Docker Startup Script
Write-Host "🚀 Starting MyLeague Backend Docker Environment..." -ForegroundColor Green

# Check if Docker is running
try {
    docker --version | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Docker not found"
    }
} catch {
    Write-Host "❌ Docker is not installed or not running!" -ForegroundColor Red
    Write-Host "Please install Docker Desktop and make sure it's running." -ForegroundColor Yellow
    exit 1
}

# Navigate to the backend directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

Write-Host "📂 Current directory: $(Get-Location)" -ForegroundColor Cyan

# Stop any existing containers
Write-Host "🛑 Stopping existing containers..." -ForegroundColor Yellow
docker-compose down

# Start the services
Write-Host "🏗️ Building and starting services..." -ForegroundColor Blue
docker-compose up -d --build

# Wait a moment for services to start
Write-Host "⏳ Waiting for services to initialize..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check service status
Write-Host "📊 Service Status:" -ForegroundColor Green
docker-compose ps

Write-Host ""
Write-Host "✅ MyLeague Backend is starting up!" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 Available Services:" -ForegroundColor Cyan
Write-Host "   • API:              http://localhost:8080" -ForegroundColor White
Write-Host "   • API Docs:         http://localhost:8080 (Swagger UI)" -ForegroundColor White
Write-Host "   • Health Check:     http://localhost:8080/health" -ForegroundColor White
Write-Host "   • Log Viewer (Seq): http://localhost:5341" -ForegroundColor White
Write-Host "   • PostgreSQL:       localhost:5432 (postgres/postgres)" -ForegroundColor White
Write-Host ""
Write-Host "📋 Useful Commands:" -ForegroundColor Cyan
Write-Host "   • View logs:        docker-compose logs -f api" -ForegroundColor White
Write-Host "   • Stop services:    docker-compose down" -ForegroundColor White
Write-Host "   • Restart:          docker-compose restart api" -ForegroundColor White
Write-Host ""
Write-Host "🎉 Setup complete! Your MyLeague backend is running." -ForegroundColor Green 