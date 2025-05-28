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
│   ├── Floorball/      # Floorball-specific entities
│   └── Hockey/         # Hockey-specific entities
├── ValueObjects/       # Immutable value objects
│   ├── Common/         # Shared value objects (Address, ContactInfo)
│   ├── Floorball/      # Floorball-specific value objects
│   └── Hockey/         # Hockey-specific value objects
├── Enums/             # Domain enumerations
│   ├── Floorball/      # Floorball-specific enums
│   └── Hockey/         # Hockey-specific enums
├── DomainEvents/      # Domain event definitions
│   ├── Common/         # Shared domain events
│   ├── Floorball/      # Floorball-specific events
│   └── Hockey/         # Hockey-specific events
├── EventSourcing/     # Event sourcing infrastructure
├── Repositories/      # Repository interface definitions
│   ├── Common/         # Shared repository interfaces
│   ├── Floorball/      # Floorball-specific repositories
│   └── Hockey/         # Hockey-specific repositories
└── DomainGlossary.md  # Ubiquitous language definitions
```

## 🎯 Core Domain Concepts

### Aggregate Roots
The following entities serve as aggregate roots in our domain model:

1. **Club** - Manages organizational membership and teams
2. **Person** - Manages individual identity and contact information
3. **FloorballSeason** - Manages competition schedules and participating teams
4. **FloorballMatch** - Manages match data, events, and state transitions
5. **FloorballTeam** - Manages team roster and details
6. **FloorballPlayer** - Manages player-specific attributes and statistics
7. **FloorballReferee** - Manages referee qualifications and assignments
8. **FloorballCoach** - Manages coaching credentials and responsibilities
9. **FloorballTeamManager** - Manages administrative team responsibilities

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

### Hockey (Future Enhancement)
- Similar structure to floorball with hockey-specific rules
- Extendable framework for additional sports

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