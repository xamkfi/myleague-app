#!/usr/bin/env pwsh

# Script to restart Docker Compose for debugging
Write-Host "🧹 Cleaning up existing containers..." -ForegroundColor Yellow

# Stop and remove containers
docker-compose down

# Remove any orphaned containers
docker-compose down --remove-orphans

# Clean up images (optional - comment out if you want to keep images)
# docker image prune -f

Write-Host "🔨 Building containers for debugging..." -ForegroundColor Blue

# Build containers with override (debugging configuration)
docker-compose -f docker-compose.yml -f docker-compose.override.yml build --no-cache api

Write-Host "🚀 Starting containers..." -ForegroundColor Green

# Start all services
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d

Write-Host "📋 Container status:" -ForegroundColor Cyan
docker-compose ps

Write-Host ""
Write-Host "✅ Containers are ready for debugging!" -ForegroundColor Green
Write-Host "   API: http://localhost:8080" -ForegroundColor White
Write-Host "   Seq: http://localhost:5341" -ForegroundColor White
Write-Host "   PostgreSQL: localhost:5432" -ForegroundColor White
Write-Host ""
Write-Host "💡 Now you can start debugging from Visual Studio 2022" -ForegroundColor Yellow 