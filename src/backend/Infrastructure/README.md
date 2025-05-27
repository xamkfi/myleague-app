# Infrastructure Layer - League Management System

## Overview

This is the **Infrastructure Layer** of the league management system, responsible for implementing technical concerns and providing concrete implementations for the abstractions defined in the Domain layer. This layer handles data persistence, external service integrations, real-time communications, dependency injection, and cross-cutting concerns.

## 🏗️ Architecture

The Infrastructure layer implements the **Dependency Inversion Principle** by providing concrete implementations of domain abstractions:

- **Data Persistence** - Entity Framework Core with PostgreSQL
- **Repository Pattern** - Concrete repository implementations
- **Unit of Work** - Transaction management and coordination
- **Event Handling** - Domain event dispatching and processing
- **Real-time Communication** - SignalR integration for live updates
- **Dependency Injection** - Service registration and configuration
- **Event Sourcing** - Event store implementations

## 🚀 Technology Stack

- **.NET 9.0** - Latest .NET framework
- **Entity Framework Core 9.0** - ORM for data persistence
- **PostgreSQL** - Primary database via Npgsql provider
- **SignalR** - Real-time web functionality
- **ASP.NET Core** - Web framework integration
- **Newtonsoft.Json** - JSON serialization

## 📁 Project Structure

```
Infrastructure/
├── Persistence/              # Data access and persistence
│   ├── Contexts/            # EF Core DbContext implementations
│   ├── Repositories/        # Repository pattern implementations
│   ├── Configurations/      # Entity Framework configurations
│   ├── EventStores/         # Event sourcing storage
│   ├── Extensions/          # EF Core extensions and helpers
│   └── UnitOfWork/          # Transaction management
├── DomainEvents/            # Domain event handling infrastructure
│   ├── Handlers/            # Specific domain event handlers
│   ├── IDomainEventHandler.cs
│   ├── IDomainEventDispatcher.cs
│   ├── DomainEventDispatcher.cs
│   ├── SignalRDomainEventHandler.cs
│   └── NotificationDomainEventHandler.cs
├── SignalR/                 # Real-time communication
│   ├── Sports/              # Sport-specific SignalR hubs
│   ├── DomainEventHub.cs    # Main SignalR hub
│   ├── DomainEventNotifier.cs
│   ├── INotificationSender.cs
│   └── SignalRNotificationSender.cs
├── DTOs/                    # Data Transfer Objects
│   └── Notifications/       # Notification DTOs
├── DependencyInjections/    # IoC container configuration
│   ├── DependencyInjection.cs
│   └── DomainEventServiceCollectionExtensions.cs
├── Migrations/              # Database migrations
└── Infrastructure.csproj    # Project configuration
```

## 🎯 Core Components

### Data Persistence
- **CommonDbContext** - Shared entities and common data
- **FloorballDbContext** - Floorball-specific data context
- **Entity Configurations** - Fluent API entity mappings
- **Repository Implementations** - Domain repository concrete classes
- **Unit of Work** - Transaction coordination across repositories

### Domain Event Infrastructure
- **DomainEventDispatcher** - Central event dispatching
- **Domain Event Handlers** - Process specific domain events
- **SignalR Integration** - Real-time event broadcasting
- **Notification System** - Event-driven notifications

### Real-time Communication
- **SignalR Hubs** - WebSocket communication endpoints
- **Event Notifier** - Push domain events to clients
- **Notification Sender** - Send structured notifications
- **Live Updates** - Real-time match and league updates

### Dependency Injection
- **Service Registration** - Configure all infrastructure services
- **Database Setup** - Entity Framework configuration
- **SignalR Setup** - Real-time communication configuration
- **Event Handling Setup** - Domain event processing pipeline

## 🔄 Event Sourcing & CQRS

### Event Store Implementation
- **Event Persistence** - Store all domain events
- **Aggregate Reconstruction** - Rebuild aggregates from events
- **Event Versioning** - Handle event schema evolution
- **Snapshot Support** - Optimize aggregate loading

### Domain Event Processing
- **Event Dispatching** - Route events to appropriate handlers
- **Cross-Aggregate Communication** - Event-driven integration
- **External System Integration** - Publish events to external services
- **Audit Trail** - Complete system activity logging

## 🌐 Real-time Features

### SignalR Integration
- **Live Match Updates** - Real-time score and event updates
- **League Notifications** - Schedule changes and announcements
- **User Notifications** - Personal alerts and messages
- **System Events** - Administrative notifications

### Notification System
- **Event-Driven** - Triggered by domain events
- **Multi-Channel** - Web, mobile, email notifications
- **Personalized** - User-specific notification preferences
- **Reliable** - Guaranteed delivery with retry mechanisms

## 💾 Database Design

### Multi-Context Architecture
- **Common Context** - Shared entities (Person, Club, Address)
- **Sport-Specific Contexts** - Floorball, Hockey entities
- **Isolation** - Clear bounded context separation
- **Scalability** - Independent scaling per sport

### Entity Framework Features
- **Code-First Migrations** - Version-controlled schema changes
- **Fluent API Configuration** - Explicit entity mappings
- **Query Optimization** - Efficient data access patterns
- **Connection Pooling** - Optimized database connections

## 🛡️ Cross-Cutting Concerns

### Error Handling
- **Exception Management** - Structured error handling
- **Logging Integration** - Comprehensive activity logging
- **Retry Policies** - Resilient external service calls
- **Circuit Breakers** - Fault tolerance patterns

### Security
- **Connection Security** - Secure database connections
- **Data Protection** - Sensitive data encryption
- **Authentication Integration** - Identity system support
- **Authorization** - Role-based access control

### Performance
- **Connection Pooling** - Efficient database usage
- **Query Optimization** - Optimized Entity Framework queries
- **Caching Strategy** - Strategic data caching
- **Async Operations** - Non-blocking I/O operations

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK
- PostgreSQL 12+ database server
- Entity Framework Core tools

### Database Setup
```bash
# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Update database with migrations
dotnet ef database update --context CommonDbContext
dotnet ef database update --context FloorballDbContext
```

### Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=myleague;Username=user;Password=password"
  },
  "SignalR": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

### Service Registration
```csharp
// In Program.cs or Startup.cs
builder.Services.AddInfrastructure(builder.Configuration);
```

## 📊 Monitoring & Observability

### Logging
- **Structured Logging** - JSON-formatted logs
- **Performance Metrics** - Database query performance
- **Error Tracking** - Exception logging and tracking
- **Audit Logs** - Complete user activity trails

### Health Checks
- **Database Health** - PostgreSQL connectivity
- **External Services** - Third-party service availability
- **SignalR Health** - Real-time communication status
- **Memory Usage** - Application resource monitoring

## 🔧 Configuration Management

### Environment-Specific Settings
- **Development** - Local database and debugging
- **Staging** - Test environment configuration
- **Production** - Optimized production settings
- **Connection Strings** - Secure credential management

### Feature Flags
- **Real-time Features** - Toggle SignalR functionality
- **Event Sourcing** - Enable/disable event sourcing
- **Caching** - Control caching behavior
- **External Integrations** - Manage third-party services

## 🤝 Integration Points

### Domain Layer
- **Repository Implementations** - Concrete data access
- **Domain Event Handling** - Process business events
- **Entity Mapping** - Domain to persistence mapping
- **Event Store** - Domain event persistence

### Application Layer
- **Data Access** - Repository pattern implementation
- **External Services** - Third-party integrations
- **Notifications** - Real-time communication
- **File Storage** - Document and media handling

### Presentation Layer
- **SignalR Hubs** - Real-time client communication
- **Health Endpoints** - System status monitoring
- **Configuration** - Runtime settings management
- **Logging** - Request/response logging

## 📚 Learn More

- Entity Framework Core documentation
- SignalR development guide
- PostgreSQL best practices
- Domain event patterns
- CQRS implementation guides

## 🤝 Contributing

When contributing to the Infrastructure layer:

1. Follow repository patterns and abstractions
2. Maintain database migration consistency
3. Test database operations thoroughly
4. Document configuration changes
5. Ensure backward compatibility
6. Monitor performance impact

## 📄 License

This project is part of the League Management System application. 