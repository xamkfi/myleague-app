# MyLeague - League Management System

[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-blue.svg)](https://docs.microsoft.com/en-us/aspnet/core/)
[![React](https://img.shields.io/badge/React-19.1-blue.svg)](https://reactjs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.8-blue.svg)](https://www.typescriptlang.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue.svg)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue.svg)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 🎯 Overview

MyLeague is a comprehensive league management system designed for organizing and managing sports leagues, with primary focus on **floorball** and extensible support for **hockey**. Built with modern .NET technologies and following Clean Architecture principles, the system provides robust functionality for managing clubs, teams, players, matches, seasons, and tournaments.

### Key Features
- 🏟️ **Multi-Sport Support** - Primary focus on floorball with hockey extensibility
- 🏗️ **Clean Architecture** - Domain-driven design with clear separation of concerns
- ⚡ **Event Sourcing** - Complete audit trail and historical data tracking
- 🔄 **CQRS Pattern** - Optimized command and query operations
- 🌐 **Modern Web API** - RESTful services with interactive documentation
- ⚛️ **React Frontend** - Modern React 19 with TypeScript and TailwindCSS
- 🌍 **Internationalization** - Multi-language support with i18next
- 📊 **Structured Logging** - Seq integration for log visualization and analysis
- 🐳 **Containerized** - Full Docker support for development and deployment
- 🧪 **Test-Driven** - Comprehensive testing strategy across all layers

## 🏗️ Architecture Overview

MyLeague follows **Clean Architecture** principles with **Domain-Driven Design (DDD)**, **Event Sourcing**, and **CQRS** patterns:

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐          │
│  │   WebAPI    │  │   React     │  │   Mobile    │          │
│  │             │  │  Frontend   │  │    (TBD)    │          │
│  └─────────────┘  └─────────────┘  └─────────────┘          │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                Application Layer                            │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐          │
│  │  Commands   │  │   Queries   │  │  Handlers   │          │
│  └─────────────┘  └─────────────┘  └─────────────┘          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐          │
│  │ Validators  │  │     DTOs    │  │  Behaviors  │          │
│  └─────────────┘  └─────────────┘  └─────────────┘          │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│              Infrastructure Layer                           │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐          │
│  │ Persistence │  │     Seq     │  │    Event    │          │
│  │   (EF Core) │  │   Logging   │  │   Handlers  │          │
│  └─────────────┘  └─────────────┘  └─────────────┘          │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                 Domain Layer                                │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐          │
│  │  Entities   │  │Value Objects│  │   Events    │          │
│  └─────────────┘  └─────────────┘  └─────────────┘          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐          │
│  │Repositories │  │    Enums    │  │Event Sourcing│         │
│  └─────────────┘  └─────────────┘  └─────────────┘          │
└─────────────────────────────────────────────────────────────┘
```

## 🛠️ Technology Stack

### Backend Technologies
- **.NET 9.0** - Latest .NET platform with enhanced performance
- **ASP.NET Core 9.0** - Web framework with minimal APIs
- **Entity Framework Core 9.0** - Object-relational mapping
- **PostgreSQL 16** - Primary database with advanced features
- **Seq** - Structured logging and log analysis platform

### Frontend Technologies
- **React 19.1** - Modern React with latest features
- **TypeScript 5.8** - Type-safe JavaScript development
- **Vite 6.3** - Fast build tool and development server
- **TailwindCSS 4.1** - Utility-first CSS framework
- **React Router 7.6** - Client-side routing
- **i18next** - Internationalization framework

### Key Frameworks & Libraries
- **MediatR 12.5** - Mediator pattern for CQRS implementation
- **FluentValidation 11.3** - Fluent interface for validation
- **AutoMapper** - Object-to-object mapping
- **Serilog 9.0** - Structured logging with multiple sinks
- **xUnit** - Testing framework with comprehensive assertions

### Development Tools
- **Docker & Docker Compose** - Containerization and orchestration
- **Scalar** - Modern OpenAPI documentation interface
- **Seq** - Log visualization and analysis platform
- **Visual Studio 2022** - Primary IDE with container support
- **pnpm** - Fast, disk space efficient package manager

## 📁 Project Structure

```
MyLeague/
├── src/
│   ├── backend/                    # .NET Backend Application
│   │   ├── Domain/                 # Core business logic and entities
│   │   │   ├── Entities/           # Domain entities (Club, Team, Player, etc.)
│   │   │   ├── ValueObjects/       # Immutable value objects
│   │   │   ├── Enums/              # Domain enumerations
│   │   │   ├── DomainEvents/       # Domain event definitions
│   │   │   ├── EventSourcing/      # Event sourcing infrastructure
│   │   │   └── Repositories/       # Repository interfaces
│   │   │
│   │   ├── Application/            # Application business logic
│   │   │   ├── Commands/           # Write operations (CQRS)
│   │   │   ├── Queries/            # Read operations (CQRS)
│   │   │   ├── Handlers/           # Command and query handlers
│   │   │   ├── DTOs/               # Data transfer objects
│   │   │   ├── Validators/         # Input validation rules
│   │   │   └── Behaviors/          # Pipeline behaviors
│   │   │
│   │   ├── Infrastructure/         # External concerns implementation
│   │   │   ├── Persistence/        # Database contexts and repositories
│   │   │   ├── DomainEvents/       # Domain event handlers
│   │   │   └── Services/           # External service integrations
│   │   │
│   │   └── WebAPI/                 # Presentation layer
│   │       ├── Controllers/        # API controllers
│   │       ├── Models/             # API-specific models
│   │       ├── Middlewares/        # Custom middleware
│   │       └── Extensions/         # Service configuration
│   │
│   └── frontend/                   # React Frontend Application
│       ├── src/                    # Source code
│       │   ├── components/         # React components
│       │   ├── pages/              # Page components
│       │   ├── hooks/              # Custom React hooks
│       │   ├── services/           # API service layer
│       │   ├── types/              # TypeScript type definitions
│       │   ├── utils/              # Utility functions
│       │   └── i18n/               # Internationalization files
│       ├── public/                 # Static assets
│       ├── package.json            # Frontend dependencies
│       ├── vite.config.ts          # Vite configuration
│       ├── tsconfig.json           # TypeScript configuration
│       └── tailwind.config.js      # TailwindCSS configuration
│
├── tests/                          # Test projects
│   └── backend/                    # Backend tests
│       ├── Domain.UnitTests/       # Domain layer tests
│       └── Application.UnitTests/  # Application layer tests
│
├── docker-compose.yml              # Docker services configuration
├── docker-compose.override.yml     # Development overrides
├── Dockerfile                     # Container definition
└── README.md                      # This file
```

## 🚀 Getting Started

### Prerequisites
- **.NET 9.0 SDK** or later
- **Node.js 18+** and **pnpm** for frontend development
- **Docker Desktop** (recommended) or local PostgreSQL
- **Visual Studio 2022 17.8+** or **Visual Studio Code**
- **Git** for version control

### Quick Start with Docker (Recommended)

1. **Clone the Repository**
   ```bash
   git clone <repository-url>
   cd MyLeague
   ```

2. **Start with Visual Studio**
   - Open `MyLeague.sln` in Visual Studio 2022
   - Right-click on the Docker Compose project
   - Select "Set as Startup Project"
   - Press **F5** or click "Docker Compose" to build and run

3. **Start with Docker Compose**
   ```bash
   docker-compose up -d
   ```

4. **Access the Application**
   - **Frontend**: http://localhost:5173
   - **API Documentation**: http://localhost:8080/scalar/v1
   - **API Health Check**: http://localhost:8080/health
   - **Log Analysis (Seq)**: http://localhost:5341
   - **Database**: localhost:5432
     - Database: myleague
     - Username: postgres
     - Password: postgres

### Manual Setup (Local Development)

#### Backend Setup
1. **Setup Database**
   ```bash
   # Install PostgreSQL locally or use Docker
   docker run --name myleague-postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=myleague -p 5432:5432 -d postgres:16
   ```

2. **Configure Connection String**
   Update `src/backend/WebAPI/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=myleague;Username=postgres;Password=postgres;Port=5432"
     }
   }
   ```

3. **Run Database Migrations**
   ```bash
   cd src/backend/Infrastructure
   dotnet ef database update --context CommonDbContext
   dotnet ef database update --context FloorballDbContext
   ```

4. **Start the API**
   ```bash
   cd src/backend/WebAPI
   dotnet run
   ```

#### Frontend Setup
1. **Install Dependencies**
   ```bash
   cd src/frontend
   pnpm install
   ```

2. **Start Development Server**
   ```bash
   pnpm dev
   ```

3. **Access the Application**
   - **Frontend**: http://localhost:5173
   - **API**: http://localhost:5000 (or https://localhost:5001)

## 🔌 API Endpoints

### Core Resources
| Resource | Base URL | Description |
|----------|----------|-------------|
| **Clubs** | `/api/clubs` | Sports club management |
| **Teams** | `/api/floorball/teams` | Floorball team operations |
| **Players** | `/api/floorball/players` | Player management |
| **Matches** | `/api/floorball/matches` | Match scheduling and results |
| **Seasons** | `/api/floorball/seasons` | Season organization |

### System Endpoints
| Endpoint | Description |
|----------|-------------|
| `/health` | Application health status |
| `/scalar/v1` | Interactive API documentation |
| `/swagger/v1/swagger.json` | OpenAPI specification |

## 📚 Layer Documentation

Each layer has comprehensive documentation with development guides:

### 🏛️ [Domain Layer](src/backend/Domain/README.md)
- **Core Business Logic** - Entities, value objects, and domain services
- **Event Sourcing** - Complete audit trail and event-driven architecture
- **Domain-Driven Design** - Rich domain models with business rules
- **Development Guide**: [FeatureDevelopmentGuide.md](src/backend/Domain/FeatureDevelopmentGuide.md)

### 🗄️ [Infrastructure Layer](src/backend/Infrastructure/README.md)
- **Data Persistence** - Entity Framework Core with PostgreSQL
- **Event Handling** - Domain event processing
- **External Services** - Third-party integrations and APIs
- **Development Guide**: [InfrastructureDevelopmentGuide.md](src/backend/Infrastructure/InfrastructureDevelopmentGuide.md)

### ⚙️ [Application Layer](src/backend/Application/README.md)
- **CQRS Implementation** - Command and query separation
- **Business Orchestration** - Application services and handlers
- **Validation & Mapping** - Input validation and object mapping
- **Development Guide**: [ApplicationDevelopmentGuide.md](src/backend/Application/ApplicationDevelopmentGuide.md)

### 🌐 [WebAPI Layer](src/backend/WebAPI/README.md)
- **REST API** - RESTful HTTP services with OpenAPI documentation
- **API Gateway** - Single entry point for all client requests
- **Error Handling** - Global exception handling and validation
- **Development Guide**: [WebAPIDevelopmentGuide.md](src/backend/WebAPI/WebAPIDevelopmentGuide.md)

## 🧪 Testing Strategy

### Testing Framework
- **xUnit** - Primary testing framework with comprehensive assertions
- **FluentAssertions** - Improved test readability and error messages
- **Moq** - Mock framework for dependency testing
- **WebApplicationFactory** - Integration testing for API endpoints

### Test Coverage
- **Unit Tests** - Individual component testing with mocking
- **Integration Tests** - End-to-end workflow validation
- **Domain Tests** - Business logic and rule verification
- **API Tests** - HTTP endpoint and contract testing

### Running Tests
```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/backend/Domain.UnitTests/
```

## 🐳 Docker Development

### Services Configuration
The application uses Docker Compose with the following services:

```yaml
services:
  webapi:          # ASP.NET Core Web API (Port 8080)
  frontend:        # React Frontend (Port 5173)
  postgres:        # PostgreSQL 16 Database (Port 5432)
  seq:             # Seq Log Analysis (Port 5341)
```

### Development Commands
```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f webapi
docker-compose logs -f frontend

# Stop services
docker-compose down

# Rebuild and start
docker-compose up --build

# Remove volumes (clean database)
docker-compose down -v
```

### Database Management
Access PostgreSQL at localhost:5432:
- **Database**: myleague
- **Username**: postgres
- **Password**: postgres

### Log Analysis
Access Seq at http://localhost:5341 for:
- **Real-time log viewing** - See logs as they happen
- **Structured logging** - Filter and search by log properties
- **Advanced queries** - Complex log analysis capabilities
- **Performance monitoring** - Track application performance

## 🔒 Security Features

### Current Implementation
- **HTTPS Enforcement** - All API endpoints secured with HTTPS
- **Input Validation** - Comprehensive validation using FluentValidation
- **CORS Configuration** - Flexible cross-origin policy management
- **Error Handling** - Secure error responses without sensitive data leakage

### Planned Enhancements
- **JWT Authentication** - Token-based authentication system
- **Role-Based Authorization** - Granular permission system
- **Rate Limiting** - API throttling and abuse prevention
- **Audit Logging** - Comprehensive security event logging

## 📊 Performance Optimizations

### Current Features
- **Async/Await** - Non-blocking I/O operations throughout
- **Database Optimization** - Efficient Entity Framework queries
- **Event Sourcing** - Optimized read models for query performance
- **Frontend Optimization** - Vite for fast builds and hot module replacement

### Monitoring & Observability
- **Structured Logging** - Serilog with Seq integration
- **Health Checks** - Comprehensive application health monitoring
- **Performance Tracking** - Request duration and resource monitoring
- **Error Tracking** - Centralized error logging and alerting

## 🌍 Internationalization

The frontend supports multiple languages through i18next:

### Supported Languages
- **English** (default)
- **Additional languages** can be easily added

### Adding New Languages
1. Create language files in `src/frontend/src/i18n/locales/`
2. Update the i18n configuration
3. Add language selection UI components

## 🤝 Contributing

### Development Workflow
1. **Fork** the repository and create a feature branch
2. **Follow** the layer-specific development guides
3. **Write** comprehensive tests for new features
4. **Document** API changes and business logic
5. **Submit** a pull request with detailed description

### Code Standards
- **Clean Architecture** - Maintain strict layer separation
- **Domain-Driven Design** - Follow DDD principles and patterns
- **Test Coverage** - Maintain minimum 80% code coverage
- **Documentation** - Update relevant documentation for changes
- **Code Review** - All changes require peer review

### Development Guidelines
- Use the provided development guides for each layer
- Follow established patterns and conventions
- Write meaningful commit messages
- Include tests for all new functionality
- Update documentation when adding features

## 📈 Roadmap

### Phase 1: Core Foundation ✅
- [x] Domain layer with event sourcing
- [x] Infrastructure with EF Core and PostgreSQL
- [x] Application layer with CQRS
- [x] Web API with comprehensive documentation
- [x] React frontend with TypeScript
- [x] Docker containerization
- [x] Structured logging with Seq

### Phase 2: Enhanced Features 🚧
- [ ] Authentication and authorization system
- [ ] Advanced reporting and analytics
- [ ] Real-time match updates
- [ ] Mobile-responsive design improvements
- [ ] Performance monitoring dashboard

### Phase 3: Scalability & Production 📋
- [ ] Microservices architecture consideration
- [ ] Kubernetes deployment configurations
- [ ] CI/CD pipeline implementation
- [ ] Load testing and optimization
- [ ] Multi-tenant support

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

## 🔗 Resources

### Documentation
- [Domain Layer Guide](src/backend/Domain/README.md)
- [Infrastructure Layer Guide](src/backend/Infrastructure/README.md)
- [Application Layer Guide](src/backend/Application/README.md)
- [WebAPI Layer Guide](src/backend/WebAPI/README.md)

### External Resources
- [.NET 9.0 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [React Documentation](https://reactjs.org/docs/)
- [TypeScript Documentation](https://www.typescriptlang.org/docs/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)

---

**MyLeague** - Building the future of sports league management with modern technology and clean architecture principles.