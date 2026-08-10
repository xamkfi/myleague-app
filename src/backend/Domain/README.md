# Domain Layer - League Management System

## Overview

This is the **Domain Layer** of a comprehensive league management system designed for ice hockey and floorball sports. The domain layer implements Domain-Driven Design (DDD) principles and includes event sourcing capabilities to manage complex business logic and maintain a complete audit trail of all domain events.

## 🏗️ Architecture

This domain layer follows **Clean Architecture** and **Domain-Driven Design** principles:

- **Entities**: Core business objects with identity and lifecycle
- **Value Objects**: Immutable objects that describe characteristics
- **Aggregates**: Clusters of related entities and value objects
- **Domain Events**: Record significant business occurrences
- **Repositories**: Abstraction for data persistence
- **Event Sourcing**: Complete event history with aggregate reconstruction

## 🚀 Technology Stack

- **.NET 9.0** - Latest .NET framework
- **C# 13** - Modern C# language features
- **Nullable Reference Types** - Enhanced null safety
- **Code Analysis** - Static analysis with Microsoft.CodeAnalysis.NetAnalyzers

## 📁 Project Structure

```
Domain/
├── Entities/           # Core business entities
│   ├── Common/         # Shared entities (Person, Club, etc.)
│   ├── Floorball/      # Floorball-specific entities (flat structure)
│   └── Hockey/         # Hockey-specific entities (grouped by subdomain)
│       ├── Competitions/
│       ├── Teams/
│       ├── Matches/
│       └── Statistics/
├── ValueObjects/       # Immutable value objects
│   ├── Common/         # Shared value objects (Address, ContactInfo)
│   ├── Floorball/      # Floorball-specific value objects
│   └── Hockey/         # Hockey-specific value objects
│       ├── Rules/
│       ├── Matches/
│       ├── Statistics/
│       └── Common/
├── Enums/             # Domain enumerations
│   ├── Common/         # Shared enums (SportsCategory, etc.)
│   ├── Floorball/      # Floorball-specific enums
│   └── Hockey/         # Hockey-specific enums
│       ├── Competitions/
│       ├── Teams/
│       ├── Matches/
│       └── Statistics/
├── Services/           # Domain service interfaces
│   └── Hockey/         # Hockey-specific services
├── Repositories/      # Repository interface definitions
│   ├── Common/         # Shared repository interfaces
│   ├── Floorball/      # Floorball-specific repositories
│   └── Hockey/         # Hockey-specific repositories
└── DomainGlossary.md  # Ubiquitous language definitions
```

### Hockey naming conventions

All ice hockey domain types use the `Hockey` prefix (e.g. `HockeyTeam`, `HockeyMatchStatus`). Namespaces follow the folder structure:

| Folder | Namespace example |
|--------|-------------------|
| `Entities/Hockey/Teams/` | `Domain.Entities.Hockey.Teams` |
| `Enums/Hockey/Matches/` | `Domain.Enums.Hockey.Matches` |
| `ValueObjects/Hockey/Rules/` | `Domain.ValueObjects.Hockey.Rules` |
| `Services/Hockey/` | `Domain.Services.Hockey` |
| `Repositories/Hockey/` | `Domain.Repositories.Hockey` |

## 🎯 Core Domain Concepts

### Aggregate Roots
The following entities serve as aggregate roots in our domain model:

1. **Club** - Manages organizational membership and teams
2. **Person** - Manages individual identity and contact information
3. **FloorballSeason** - Manages competition schedules and participating teams
4. **FloorballMatch** - Manages match data, events, and state transitions
5. **FloorballTeam** - Manages team roster and details
6. **FloorballPlayer** - Manages player-specific attributes and statistics
7. **FloorballTeamManager** - Manages team operations and administration
8. **FloorballReferee** - Officiates matches with licensing and experience tracking

### Key Value Objects
- **Address** - Physical location information
- **ContactInfo** - Email and phone contact details
- **Position** - Player position preferences and assignments
- **Score** - Match scoring information
- **FloorballTeamPlayer** - Player-team association with statistics

### Domain Events
- **Match Events**: Creation, scheduling, status changes
- **Game Events**: Goals scored, penalties assigned
- **Assignment Events**: Official and player assignments

## 🔄 Event Sourcing

The domain includes sophisticated event sourcing capabilities:

- **EventSourcedAggregate** - Base class for event-sourced aggregates
- **AggregateRoot** - Foundation for traditional aggregates
- **IEventStore** - Interface for event persistence
- **EventSourcedFloorballMatch** - Full event-sourced match implementation

### Benefits of Event Sourcing
- **Complete Audit Trail** - Every state change is recorded
- **Temporal Queries** - Query system state at any point in time
- **Replay Capability** - Reconstruct aggregates from events
- **Integration** - Easy integration with external systems via events

## 📖 Ubiquitous Language

The domain uses a carefully crafted **ubiquitous language** shared between developers and domain experts. All terms are documented in the `DomainGlossary.md` file, ensuring consistent communication and understanding across the team.

## 🏒 Sports Support

### Floorball (Primary Focus)
- Complete match management with periods and events
- Player positions: Forward, Center, Defender, Goalkeeper
- Comprehensive penalty system
- Official assignment and management
- Team and league administration

### Hockey (In Development)
- Domain folder structure in place; entities and services to be added incrementally
- Follows Floorball patterns with grouped subfolders under `Hockey/`
- All types prefixed with `Hockey`

## 🛡️ Code Quality

This project maintains high code quality standards:

- **Nullable Reference Types** enabled for null safety
- **Code Style Enforcement** during build
- **Comprehensive Analysis** with all Microsoft analyzers
- **Consistent Formatting** and naming conventions

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK or later
- IDE with C# support (Visual Studio, VS Code, Rider)

### Building the Project
```bash
dotnet build
```

### Running Tests
```bash
dotnet test
```

## 📚 Learn More

- Review the `DomainGlossary.md` for detailed domain terminology
- Explore entity implementations to understand business rules
- Study event sourcing patterns in the EventSourcing folder
- Check domain events for integration patterns

## 🤝 Contributing

When contributing to this domain layer:

1. Follow DDD principles and maintain aggregate boundaries
2. Update the domain glossary when introducing new concepts
3. Ensure all domain events are properly defined
4. Maintain ubiquitous language consistency
5. Add comprehensive unit tests for business logic

## 📄 License

This project is part of the League Management System application. 