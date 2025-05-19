# MyLeague Development Plan

## Overview
This document outlines the development roadmap for the MyLeague application, covering backend, frontend, and cloud infrastructure phases. The application will be built using a phased approach to ensure organized and manageable progress.

## Phase 1: Foundation & Backend Setup

### Backend
- Set up project structure with proper architecture (Domain, Infrastructure, API, App layers)
- Implement core domain models:
  - Club
  - Team
  - Player
  - Person
  - League
  - Match
  - News
- Create database schema and Entity Framework configurations for PostgreSQL
- Implement Redis for caching and session management
- Implement basic repository pattern for data access
- Develop core services for entity management
- Create initial API endpoints:
  - GET endpoints for browsing entities
  - POST/PUT/DELETE endpoints for admin operations
- Implement JWT-based authentication framework

### Cloud
- Set up CI/CD pipeline structure
  - Configure separate pipelines for frontend and backend
  - Configure triggers based on monorepo folder changes
- Set up development environment in Azure
  - App Services for backend
  - Static Web App for frontend
  - PostgreSQL in Docker (stellirin/postgres-windows) for database
  - Redis in Docker (jkcomsolidation/redis-windows) for caching
- Implement containerization with Docker for all services
  - Use Docker Compose for local development
  - Use Azure Container Registry for image storage
- Configure secrets management in Azure Key Vault
  - Database connection strings
  - Redis connection strings
  - JWT configuration (issuer, audience, key)
  - Other sensitive configurations

## Phase 2: Frontend Development & Admin Dashboard

### Frontend
- Set up frontend project structure with modern framework
- Implement admin dashboard:
  - Club management (create, edit, delete)
  - Team management
  - Player management
  - League management
  - Person management
- Develop match creation and updates:
  - Home/away team selection
  - Match details entry
  - Support for multiple sports (floorball and football initially)
- Implement results browsing:
  - Match results view
  - League table view
  - Player statistics view
- Create news management system:
  - News article creation with text and images
  - Enhanced feature: Link/embed match statistics in news articles
  - Reference implementation similar to http://mahl.fi/index.php?option=com_content&view=article&id=1354:loeysae-ja-alapiha-salibandyn-mestarit&catid=11:slider&Itemid=27

### Backend
- Extend API endpoints to support frontend requirements
- Implement proper data validation
- Add filtering and pagination to API responses
- Create specialized endpoints for statistics and tables

## Phase 3: Security, Authorization & Advanced Features

### Backend
- Enhance security with proper authorization:
  - Implement role-based access control (Admin, Team Manager, User)
  - Secure admin-only endpoints:
    - Club management
    - Team management
    - Player management
    - League management
- Implement more advanced query optimization
- Enhance Redis caching mechanisms for frequently accessed data
- Create background jobs for processing statistics and standings

### Frontend
- Implement user authentication UI
- Create role-specific views and features
- Develop advanced filtering and search functionality
- Add real-time updates for match results
- Create mobile-responsive design improvements

### Cloud
- Set up production environment in Azure
- Configure staging deployment slots
- Implement database backup strategy for PostgreSQL containers
- Implement Redis persistence configuration
- Set up monitoring and alerting:
  - Application Insights
  - Azure Monitor
  - Health checks
- Configure scaling rules for handling traffic spikes

## Phase 4: Refinement & Extended Features

### Backend & Frontend
- Implement additional sports support beyond floorball and football
- Add season management and historical data views
- Create tournament/cup competition support
- Implement player profile pages with career statistics
- Add team management features for team managers
- Develop notification system for important events
- Create API for potential mobile application

### Cloud
- Add CDN for static content delivery <- not high importance
- Optimize PostgreSQL database performance
- Implement disaster recovery plan
- Set up cost monitoring and optimization

## Technology Stack

### Backend
- ASP.NET Core
- Entity Framework Core
- PostgreSQL (stellirin/postgres-windows:16)
- Redis (jkcomsolidation/redis-windows)
- JWT authentication

### Frontend
- Modern JavaScript framework (React, Angular, or Vue)
- Responsive design framework
- State management solution

### Cloud (Azure)
- Azure App Service
- Azure Container Instances for containerized services
- Azure DevOps for CI/CD
- Azure Key Vault
- Azure Blob Storage for images
- Application Insights
- Azure Container Registry 