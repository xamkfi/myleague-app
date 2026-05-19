# MyLeague - League Management System

[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-blue.svg)](https://docs.microsoft.com/en-us/aspnet/core/)
[![React](https://img.shields.io/badge/React-19.1-blue.svg)](https://reactjs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.8-blue.svg)](https://www.typescriptlang.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue.svg)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue.svg)](https://www.docker.com/)
[![Backend CI](https://github.com/xamk-ture/MyLeague-app/actions/workflows/backend-ci.yaml/badge.svg)](https://github.com/xamk-ture/MyLeague-app/actions/workflows/backend-ci.yaml)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 🎯 Overview

MyLeague is a comprehensive league management system designed for organizing and managing sports leagues, with primary focus on **floorball** and extensible support for **hockey**. Built with modern .NET technologies and following Clean Architecture principles, the system provides robust functionality for managing clubs, teams, players, matches, seasons, and tournaments.

### Key Features
- 🏟️ **Multi-Sport Support** - Primary focus on floorball with hockey extensibility
- 🏆 **Seasons & Tournaments** - Unified `FloorballCompetition` (TPH) base for league seasons and group/playoff tournaments, with shared match/statistics flow
- 📅 **Event Calendar** - Cross-sport calendar of upcoming and past matches
- 📰 **News & Editorial** - Hero carousel and category-tagged news articles managed in-app
- 📈 **Statistics & Standings** - Per-season and per-group standings, top scorers, team/player season stats, and live match stats
- 🏗️ **Clean Architecture** - Domain-driven design with clear separation of concerns
- ⚡ **Event Sourcing** - Complete audit trail and historical data tracking
- 🔄 **CQRS Pattern** - Optimized command and query operations via MediatR
- 🌐 **Modern Web API** - RESTful services with Scalar/OpenAPI interactive documentation
- ⚛️ **React Frontend** - React 19 + TypeScript + Vite, with reusable design tokens and SCSS
- 🔐 **Passwordless Auth** - Email-code login with JWT access + refresh-token rotation
- 🌍 **Internationalization** - Finnish/English UI with i18next
- 📊 **Structured Logging** - Serilog + Seq integration for log visualization and analysis
- 🐳 **Containerized** - Full Docker support for development and deployment
- 🧪 **Test-Driven** - xUnit + FluentAssertions across Domain and Application layers
- 🌱 **Database Seeder** - Standalone tool that bootstraps a complete dev dataset (clubs, teams, players, referees, seasons, tournaments, matches, simulated stats) — see [Database Seeding](#-database-seeding)

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

> **Domain note — competitions:** `FloorballSeason` and `FloorballTournament` both inherit from `FloorballCompetition` and are persisted as a single PostgreSQL table using EF Core's **Table-Per-Hierarchy (TPH)** strategy. Matches, statistics, and standings reference a competition by `CompetitionId`, so the same query/handler stack serves both league seasons and tournaments.

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
│   │   │   ├── Entities/           # Domain entities (Club, Team, Player,
│   │   │   │                       # FloorballCompetition (TPH base),
│   │   │   │                       # FloorballSeason, FloorballTournament, etc.)
│   │   │   ├── ValueObjects/       # Immutable value objects (e.g. FloorballMatchRules)
│   │   │   ├── Enums/              # Domain enumerations (incl. tournament lifecycle)
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
│   ├── tools/                      # Development & data-import tools
│   │   ├── Seeder/                 # HTTP-based dev database seeder (clubs, teams,
│   │   │                           # players, referees, seasons, tournaments, matches)
│   │   ├── FloorballPlayerImporter/# Bulk player import from JSON team rosters
│   │   ├── MahlImporter/           # Scrape & import historical MAHL data
│   │   └── DataImporter/           # Legacy JLG XML person import
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

4. **Seed Initial Data** (optional but recommended)

   After the services are running, populate the database with a complete dev dataset (clubs, teams, players, referees, seasons, **tournaments**, matches with simulated stats):
   ```bash
   dotnet run --project src/tools/Seeder/Seeder.csproj
   ```
   The seeder is idempotent — see the dedicated [Database Seeding](#-database-seeding) section below for what gets created and how to troubleshoot.

5. **Access the Application**
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

5. **Seed Initial Data** (optional but recommended)

   In a separate terminal, run the seeder to populate the database with the standard dev dataset:
   ```bash
   dotnet run --project src/tools/Seeder/Seeder.csproj
   ```
   See the [Database Seeding](#-database-seeding) section below for the full breakdown.

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
   - **API**: http://localhost:65533 (or https://localhost:65532)

   > **Note:** When running the backend locally with `dotnet run`, the frontend's `VITE_API_URL` must match the backend port. Update `src/frontend/.env.development` if needed:
   > ```
   > VITE_API_URL=http://localhost:65533/api
   > ```
   > When using Docker, the default value (`http://localhost:8080/api`) works out of the box.

## 🔌 API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | Request a login code (sent to email) |
| POST | `/api/auth/verify` | Verify login code, receive JWT + refresh token |
| POST | `/api/auth/refresh` | Refresh tokens using a valid refresh token |
| POST | `/api/auth/logout` | Revoke a refresh token (logout) |
| GET | `/api/auth/me` | Get current authenticated user info |

### Common Resources
| Resource | Base URL | Description |
|----------|----------|-------------|
| **Clubs** | `/api/clubs` | Sports club management |
| **Divisions** | `/api/divisions` | Division/league hierarchy |
| **Persons** | `/api/persons` | Person profiles (used by players, referees, managers) |
| **Users** | `/api/users` | User management (requires auth) |
| **News** | `/api/news` | News articles and hero carousel content |
| **Search** | `/api/search` | Cross-entity search |

### Floorball Resources
| Resource | Base URL | Description |
|----------|----------|-------------|
| **Teams** | `/api/floorballteam` | Floorball team and roster operations |
| **Players** | `/api/floorballplayer` | Player profiles and team assignments |
| **Referees** | `/api/floorballreferee` | Referee profiles and license info |
| **Seasons** | `/api/floorballseason` | League season organization |
| **Tournaments** | `/api/floorballtournament` | Tournaments with groups, group teams, lifecycle (draft → registration → group stage → playoff → completed) |
| **Matches** | `/api/floorballmatch` | Match scheduling, live state, goals/penalties/saves, and results |
| **Statistics** | `/api/floorball/statistics` | Standings (season + tournament group), top scorers, season summaries, per-match stats |

> Tournaments and seasons share the `FloorballCompetition` (TPH) base, so a `competitionId` works uniformly across match, statistics, and standings endpoints regardless of whether it points to a season or a tournament.

### System Endpoints
| Endpoint | Description |
|----------|-------------|
| `/health` | Application health status |
| `/scalar/v1` | Interactive API documentation |
| `/swagger/v1/swagger.json` | OpenAPI specification |

## 🌱 Database Seeding

A standalone .NET console tool at [`src/tools/Seeder`](src/tools/Seeder/README.md) populates a clean database with a complete, realistic dataset by calling the running WebAPI through HTTP. It's the fastest way to get a usable dev environment after a fresh `docker-compose up` (or after dropping the DB volume).

### What gets seeded

The seeder creates entities in a strict order so foreign keys resolve naturally:

1. **Persons** — base persons, players, goalies, and referees
2. **Clubs** (10) — Finnish floorball clubs with logos and contact info
3. **Divisions** — league/division hierarchy
4. **Floorball Players** (~100) — player registrations
5. **Floorball Referees** — referee profiles with license dates anchored to "now"
6. **Seasons** — league season(s) with division associations and match-rule defaults
7. **Teams** — teams assigned to clubs and divisions
8. **Team-Season assignments** — divisional placement
9. **Player-Team assignments** — populated rosters with jersey numbers and positions
10. **League Matches** — scheduled matches per season; some are simulated through to completion to populate stats
11. **Tournaments** — `FloorballTournament` with groups, group-teams, and dynamic dates relative to "now" (so the listing always shows a sensible "Tulossa / Käynnissä / Päättynyt" status)
12. **Tournament Matches** — round-robin group-stage matches anchored around "now"; past-dated matches are simulated through to completion (goals + saves + completion event), future-dated matches stay in `Scheduled` state

### Run

After the WebAPI is up (Docker or local `dotnet run`):

```bash
dotnet run --project src/tools/Seeder/Seeder.csproj
```

The seeder will prompt for the API base URL (default `http://localhost:8080/`), authenticate as the dev admin (`test@myleague.local`), and report progress for each step. A summary at the end prints how many entities were created or already existed.

### Idempotency & re-runs

Every step performs an existence check before creating anything (by email/name/composite key), so the seeder is safe to run repeatedly. Re-runs also **refresh the tournament's date window** so the seeded tournament always overlaps "now" and matches stay in a believable mix of completed/upcoming states.

If the API responses look wrong or empty (e.g. pagination silently returns no records), the seeder now prints loud `WARNING:` lines to `stderr` with the raw HTTP status and response body — see [Seeder/README](src/tools/Seeder/README.md) for the full diagnostic flow and the test data layout in `data/testdata.json`.

### Other tools under `src/tools/`

| Tool | Purpose |
|------|---------|
| [`Seeder/`](src/tools/Seeder/README.md) | Populate a dev database via HTTP (described above) |
| [`MahlImporter/`](src/tools/MahlImporter/) | Scrape and import historical MAHL match/team data |
| [`FloorballPlayerImporter/`](src/tools/FloorballPlayerImporter/README.md) | Bulk-import floorball players from JSON team rosters |
| `DataImporter/` | Import legacy person data from JLG XML files |

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

## 🔒 Security & Authentication

### Passwordless Email Authentication
MyLeague uses a passwordless login system -- no passwords are stored in the database. The authentication flow works as follows:

1. **Request code** -- User submits their email to `POST /api/auth/login`
2. **Receive code** -- A 6-digit code is sent to the email (logged to console in development, sent via Azure Communication Services Email in production)
3. **Verify code** -- User submits the code to `POST /api/auth/verify` and receives a JWT access token (short-lived, 15 min) and a refresh token (long-lived, 7 days)
4. **Use token** -- Include the access token as `Authorization: Bearer <token>` on protected endpoints
5. **Refresh** -- When the access token expires, call `POST /api/auth/refresh` with the refresh token to get a new pair
6. **Logout** -- Call `POST /api/auth/logout` to revoke the refresh token

### Security Features
- **Passwordless** -- No passwords stored; login codes are cryptographically generated and short-lived (10 min)
- **Brute-force protection** -- Login codes lock after 5 failed attempts; user must request a new code
- **JWT authentication** -- Short-lived access tokens with claims (userId, email, personId, role)
- **Refresh token rotation** -- Each refresh revokes the old token and issues a new one; reuse of a revoked token revokes all tokens for that user (theft detection)
- **Secure storage** -- Only SHA256 hashes of refresh tokens are stored in the database
- **HTTPS enforcement** -- All API endpoints secured with HTTPS
- **Input validation** -- Comprehensive validation using FluentValidation
- **CORS configuration** -- Flexible cross-origin policy management
- **Error handling** -- Secure error responses without sensitive data leakage

### Default Users & Seeding
- **Local development (dotnet run)** -- A test user (`test@myleague.local`, role: Admin) is automatically created on first startup. Request a login code and find it in the console output.
- **Docker development** -- The Docker Compose override sets `Seed__AdminEmail=test@myleague.fi`. Use this email to log in. Find the login code in the container logs: `docker-compose logs -f webapi`
- **Production / Azure** -- Set the `Seed__AdminEmail` environment variable (e.g., `admin@yourdomain.com`) in Azure App Service. An admin user will be created on first startup if it does not already exist.

### Planned Enhancements
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
- **Finnish** (default)
- **English**
- Additional languages can be added by dropping a JSON file into `src/frontend/src/i18n/locales/` and registering it in the i18n setup.

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
- [x] Passwordless email authentication with JWT
- [x] Refresh token rotation with theft detection
- [x] Database seeding for dev and production admin users
- [x] Standalone HTTP seeder tool with idempotent test dataset
- [x] `FloorballCompetition` (TPH) refactor — unified seasons & tournaments
- [x] Tournament management (groups, group-teams, lifecycle, group standings)
- [x] Public tournament listing & detail pages with full statistics
- [x] News module (hero carousel + categorized articles)
- [x] Cross-sport event calendar of upcoming matches
- [x] Statistics module (standings, top scorers, team/player season stats)
- [x] GitHub Actions backend CI
- [ ] Real-time match updates (live scoreboard via WebSockets/SSE)
- [ ] Advanced reporting and analytics dashboards
- [ ] Mobile-responsive design improvements
- [ ] Performance monitoring dashboard

### Phase 3: Scalability & Production 📋
- [ ] Microservices architecture consideration
- [ ] Kubernetes deployment configurations
- [ ] Full CI/CD pipeline (deploy + frontend CI)
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