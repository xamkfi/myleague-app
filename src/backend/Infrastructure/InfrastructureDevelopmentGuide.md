# Infrastructure Development Guide - League Management System

## Overview

This guide provides comprehensive instructions for developing new infrastructure features while maintaining clean architecture principles, proper separation of concerns, and technical excellence.

## 🎯 Before You Start

### Prerequisites
- Understanding of Clean Architecture principles
- Knowledge of Entity Framework Core and PostgreSQL
- Familiarity with SignalR and real-time communication
- Experience with dependency injection patterns
- Understanding of CQRS and Event Sourcing concepts

### Key Principles to Follow
1. **Dependency Inversion** - Implement domain abstractions
2. **Separation of Concerns** - Each component has a single responsibility
3. **Configuration Management** - Externalize all configuration
4. **Performance** - Optimize database queries and async operations
5. **Resilience** - Handle failures gracefully with retry policies

## 🚀 Development Process

### Step 1: Infrastructure Analysis & Design

#### 1.1 Understand the Technical Requirement
- [ ] Identify the technical problem being solved
- [ ] Determine required infrastructure components
- [ ] Assess performance and scalability requirements
- [ ] Plan database schema changes if needed
- [ ] Consider real-time communication requirements

#### 1.2 Technical Design
- [ ] Design database entities and relationships
- [ ] Plan repository interfaces implementation
- [ ] Design domain event handlers
- [ ] Plan SignalR integration if needed
- [ ] Consider caching strategies

#### 1.3 Implementation Planning
- [ ] Plan database migrations
- [ ] Design service registrations
- [ ] Plan integration testing strategy
- [ ] Consider monitoring and logging requirements

### Step 2: Implementation

#### 2.1 Database Context & Entities

**For New Entities:**
```csharp
// Entity Configuration
namespace Infrastructure.Persistence.Configurations.Floorball
{
    public class NewEntityConfiguration : IEntityTypeConfiguration<NewEntity>
    {
        public void Configure(EntityTypeBuilder<NewEntity> builder)
        {
            builder.ToTable("NewEntities", "floorball");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Id)
                .HasConversion(
                    id => id.Value,
                    value => new EntityId(value))
                .ValueGeneratedNever();
                
            builder.Property(e => e.Name)
                .HasMaxLength(200)
                .IsRequired();
                
            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            // Value object configuration
            builder.OwnsOne(e => e.Address, address =>
            {
                address.Property(a => a.Street).HasMaxLength(100);
                address.Property(a => a.City).HasMaxLength(50);
                address.Property(a => a.PostalCode).HasMaxLength(20);
                address.Property(a => a.Country).HasMaxLength(50);
            });
            
            // Relationships
            builder.HasOne(e => e.RelatedEntity)
                .WithMany(r => r.NewEntities)
                .HasForeignKey(e => e.RelatedEntityId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

// Add to DbContext
namespace Infrastructure.Persistence.Contexts
{
    public class FloorballDbContext : DbContext
    {
        public DbSet<NewEntity> NewEntities { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new NewEntityConfiguration());
        }
    }
}
```

#### 2.2 Repository Implementation (Implements the Domain repository interfaces)

```csharp
namespace Infrastructure.Persistence.Repositories.Floorball
{
    public class NewEntityRepository : RepositoryBase<NewEntity, EntityId>, INewEntityRepository
    {
        private readonly FloorballDbContext _context;

        public NewEntityRepository(FloorballDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<NewEntity?> GetByIdAsync(EntityId id, CancellationToken cancellationToken = default)
        {
            return await _context.NewEntities
                .Include(e => e.RelatedEntity)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<NewEntity>> GetByConditionAsync(
            string? searchTerm = null,
            int page = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var query = _context.NewEntities.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(e => e.Name.Contains(searchTerm));
            }

            return await query
                .OrderBy(e => e.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(EntityId id, CancellationToken cancellationToken = default)
        {
            return await _context.NewEntities
                .AnyAsync(e => e.Id == id, cancellationToken);
        }

        public async Task SaveAsync(NewEntity entity, CancellationToken cancellationToken = default)
        {
            if (await ExistsAsync(entity.Id, cancellationToken))
            {
                _context.NewEntities.Update(entity);
            }
            else
            {
                await _context.NewEntities.AddAsync(entity, cancellationToken);
            }
        }

        public async Task DeleteAsync(EntityId id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity != null)
            {
                _context.NewEntities.Remove(entity);
            }
        }
    }
}
```

#### 2.3 Domain Event Handlers

```csharp
namespace Infrastructure.DomainEvents.Handlers.Floorball
{
    public class NewEntityCreatedEventHandler : IDomainEventHandler<NewEntityCreatedEvent>
    {
        private readonly ILogger<NewEntityCreatedEventHandler> _logger;
        private readonly INotificationSender _notificationSender;

        public NewEntityCreatedEventHandler(
            ILogger<NewEntityCreatedEventHandler> logger,
            INotificationSender notificationSender)
        {
            _logger = logger;
            _notificationSender = notificationSender;
        }

        public async Task Handle(NewEntityCreatedEvent domainEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NewEntityCreatedEvent for entity {EntityId}", 
                domainEvent.EntityId);

            try
            {
                // Send real-time notification
                await _notificationSender.SendNotificationAsync(
                    "NewEntityCreated",
                    new NewEntityCreatedNotification(
                        domainEvent.EntityId.Value,
                        domainEvent.Name,
                        domainEvent.OccurredAt),
                    cancellationToken);

                // Additional processing...
                // - Send emails
                // - Update caches
                // - Trigger workflows

                _logger.LogInformation("Successfully handled NewEntityCreatedEvent for entity {EntityId}", 
                    domainEvent.EntityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling NewEntityCreatedEvent for entity {EntityId}", 
                    domainEvent.EntityId);
                throw;
            }
        }
    }
}
```

#### 2.4 SignalR Integration

```csharp
// SignalR Hub
namespace Infrastructure.SignalR.Sports.Floorball
{
    public class FloorballNewEntityHub : Hub
    {
        private readonly ILogger<FloorballNewEntityHub> _logger;

        public FloorballNewEntityHub(ILogger<FloorballNewEntityHub> logger)
        {
            _logger = logger;
        }

        public async Task JoinNewEntityGroup(string entityId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"NewEntity_{entityId}");
            _logger.LogInformation("Client {ConnectionId} joined group NewEntity_{EntityId}", 
                Context.ConnectionId, entityId);
        }

        public async Task LeaveNewEntityGroup(string entityId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"NewEntity_{entityId}");
            _logger.LogInformation("Client {ConnectionId} left group NewEntity_{EntityId}", 
                Context.ConnectionId, entityId);
        }
    }
}

// Notification Sender Extension
namespace Infrastructure.SignalR
{
    public class NewEntityNotificationSender : INewEntityNotificationSender
    {
        private readonly IHubContext<FloorballNewEntityHub> _hubContext;
        private readonly ILogger<NewEntityNotificationSender> _logger;

        public NewEntityNotificationSender(
            IHubContext<FloorballNewEntityHub> hubContext,
            ILogger<NewEntityNotificationSender> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyEntityCreatedAsync(EntityId entityId, string name, CancellationToken cancellationToken = default)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync(
                    "NewEntityCreated",
                    new { EntityId = entityId.Value, Name = name },
                    cancellationToken);

                _logger.LogInformation("Sent NewEntityCreated notification for entity {EntityId}", entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send NewEntityCreated notification for entity {EntityId}", entityId);
                throw;
            }
        }
    }
}
```

#### 2.5 Database Migrations

```bash
# Create migration for new entity
dotnet ef migrations add AddNewEntity --context FloorballDbContext --output-dir Migrations/Floorball

# Review and modify migration if needed
# Then update database
dotnet ef database update --context FloorballDbContext

#Or use package manager console
add-migration Test --context CommonDbContext
```

#### 2.6 Service Registration

```csharp
namespace Infrastructure.DependencyInjections
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Existing registrations...

            // New repository registration
            services.AddScoped<INewEntityRepository, NewEntityRepository>();

            // New domain event handler registration
            services.AddScoped<IDomainEventHandler<NewEntityCreatedEvent>, NewEntityCreatedEventHandler>();

            // New SignalR services
            services.AddScoped<INewEntityNotificationSender, NewEntityNotificationSender>();

            return services;
        }
    }
}
```

### Step 3: Event Sourcing Implementation (If Applicable)

#### 3.1 Event Store Implementation

```csharp
namespace Infrastructure.Persistence.EventStores
{
    public class NewEntityEventStore : INewEntityEventStore
    {
        private readonly FloorballDbContext _context;
        private readonly ILogger<NewEntityEventStore> _logger;

        public NewEntityEventStore(FloorballDbContext context, ILogger<NewEntityEventStore> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SaveEventsAsync(EntityId aggregateId, IEnumerable<IDomainEvent> events, 
            int expectedVersion, CancellationToken cancellationToken = default)
        {
            var eventList = events.ToList();
            if (!eventList.Any()) return;

            try
            {
                var eventEntities = eventList.Select((evt, index) => new EventEntity
                {
                    AggregateId = aggregateId.Value,
                    EventType = evt.GetType().Name,
                    EventData = JsonConvert.SerializeObject(evt),
                    Version = expectedVersion + index + 1,
                    OccurredAt = evt.OccurredAt
                }).ToList();

                await _context.Events.AddRangeAsync(eventEntities, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Saved {EventCount} events for aggregate {AggregateId}", 
                    eventList.Count, aggregateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save events for aggregate {AggregateId}", aggregateId);
                throw;
            }
        }

        public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(EntityId aggregateId, 
            CancellationToken cancellationToken = default)
        {
            var eventEntities = await _context.Events
                .Where(e => e.AggregateId == aggregateId.Value)
                .OrderBy(e => e.Version)
                .ToListAsync(cancellationToken);

            return eventEntities.Select(DeserializeEvent).Where(e => e != null).Cast<IDomainEvent>();
        }

        private IDomainEvent? DeserializeEvent(EventEntity eventEntity)
        {
            try
            {
                var eventType = Type.GetType($"Domain.DomainEvents.{eventEntity.EventType}");
                if (eventType == null) return null;

                return (IDomainEvent?)JsonConvert.DeserializeObject(eventEntity.EventData, eventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize event {EventType} for aggregate {AggregateId}", 
                    eventEntity.EventType, eventEntity.AggregateId);
                return null;
            }
        }
    }
}
```

### Step 4: Testing

#### 4.1 Repository Tests

```csharp
public class NewEntityRepositoryTests : IDisposable
{
    private readonly FloorballDbContext _context;
    private readonly NewEntityRepository _repository;

    public NewEntityRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FloorballDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new FloorballDbContext(options);
        _repository = new NewEntityRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingEntity_ShouldReturnEntity()
    {
        // Arrange
        var entity = new NewEntity(new EntityId(Guid.NewGuid()), "Test Entity");
        await _context.NewEntities.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(entity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
        result.Name.Should().Be("Test Entity");
    }

    [Fact]
    public async Task SaveAsync_WithNewEntity_ShouldAddToDatabase()
    {
        // Arrange
        var entity = new NewEntity(new EntityId(Guid.NewGuid()), "New Entity");

        // Act
        await _repository.SaveAsync(entity);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.NewEntities.FindAsync(entity.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("New Entity");
    }

    [Fact]
    public async Task GetByConditionAsync_WithSearchTerm_ShouldFilterResults()
    {
        // Arrange
        var entity1 = new NewEntity(new EntityId(Guid.NewGuid()), "Test Entity One");
        var entity2 = new NewEntity(new EntityId(Guid.NewGuid()), "Another Entity");
        var entity3 = new NewEntity(new EntityId(Guid.NewGuid()), "Test Entity Two");

        await _context.NewEntities.AddRangeAsync(entity1, entity2, entity3);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetByConditionAsync("Test");

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(e => e.Name == "Test Entity One");
        results.Should().Contain(e => e.Name == "Test Entity Two");
    }

    [Fact]
    public async Task ExistsAsync_WithExistingEntity_ShouldReturnTrue()
    {
        // Arrange
        var entity = new NewEntity(new EntityId(Guid.NewGuid()), "Test Entity");
        await _context.NewEntities.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync(entity.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentEntity_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = new EntityId(Guid.NewGuid());

        // Act
        var exists = await _repository.ExistsAsync(nonExistentId);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingEntity_ShouldRemoveFromDatabase()
    {
        // Arrange
        var entity = new NewEntity(new EntityId(Guid.NewGuid()), "Test Entity");
        await _context.NewEntities.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(entity.Id);
        await _context.SaveChangesAsync();

        // Assert
        var deleted = await _context.NewEntities.FindAsync(entity.Id);
        deleted.Should().BeNull();
    }
}
```

#### 4.2 Domain Event Handler Tests

```csharp
public class NewEntityCreatedEventHandlerTests
{
    private readonly Mock<ILogger<NewEntityCreatedEventHandler>> _loggerMock;
    private readonly Mock<INotificationSender> _notificationSenderMock;
    private readonly NewEntityCreatedEventHandler _handler;

    public NewEntityCreatedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<NewEntityCreatedEventHandler>>();
        _notificationSenderMock = new Mock<INotificationSender>();
        _handler = new NewEntityCreatedEventHandler(_loggerMock.Object, _notificationSenderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidEvent_ShouldSendNotification()
    {
        // Arrange
        var domainEvent = new NewEntityCreatedEvent(new EntityId(Guid.NewGuid()), "Test Entity");

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _notificationSenderMock.Verify(
            x => x.SendNotificationAsync(
                "NewEntityCreated",
                It.IsAny<NewEntityCreatedNotification>(),
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidEvent_ShouldLogInformation()
    {
        // Arrange
        var domainEvent = new NewEntityCreatedEvent(new EntityId(Guid.NewGuid()), "Test Entity");

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling NewEntityCreatedEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_WhenNotificationFails_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var domainEvent = new NewEntityCreatedEvent(new EntityId(Guid.NewGuid()), "Test Entity");
        var expectedException = new InvalidOperationException("Notification failed");
        
        _notificationSenderMock
            .Setup(x => x.SendNotificationAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(domainEvent, CancellationToken.None));

        exception.Should().Be(expectedException);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error handling NewEntityCreatedEvent")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
```

#### 4.3 Event Store Tests

```csharp
public class NewEntityEventStoreTests : IDisposable
{
    private readonly FloorballDbContext _context;
    private readonly Mock<ILogger<NewEntityEventStore>> _loggerMock;
    private readonly NewEntityEventStore _eventStore;

    public NewEntityEventStoreTests()
    {
        var options = new DbContextOptionsBuilder<FloorballDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new FloorballDbContext(options);
        _loggerMock = new Mock<ILogger<NewEntityEventStore>>();
        _eventStore = new NewEntityEventStore(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task SaveEventsAsync_WithValidEvents_ShouldPersistEvents()
    {
        // Arrange
        var aggregateId = new EntityId(Guid.NewGuid());
        var events = new List<IDomainEvent>
        {
            new NewEntityCreatedEvent(aggregateId, "Test Entity"),
            new NewEntityNameChangedEvent(aggregateId, "Test Entity", "Updated Entity")
        };

        // Act
        await _eventStore.SaveEventsAsync(aggregateId, events, 0);

        // Assert
        var savedEvents = await _context.Events
            .Where(e => e.AggregateId == aggregateId.Value)
            .OrderBy(e => e.Version)
            .ToListAsync();

        savedEvents.Should().HaveCount(2);
        savedEvents[0].EventType.Should().Be("NewEntityCreatedEvent");
        savedEvents[0].Version.Should().Be(1);
        savedEvents[1].EventType.Should().Be("NewEntityNameChangedEvent");
        savedEvents[1].Version.Should().Be(2);
    }

    [Fact]
    public async Task GetEventsAsync_WithExistingEvents_ShouldReturnOrderedEvents()
    {
        // Arrange
        var aggregateId = new EntityId(Guid.NewGuid());
        var events = new List<IDomainEvent>
        {
            new NewEntityCreatedEvent(aggregateId, "Test Entity"),
            new NewEntityNameChangedEvent(aggregateId, "Test Entity", "Updated Entity")
        };

        await _eventStore.SaveEventsAsync(aggregateId, events, 0);

        // Act
        var retrievedEvents = await _eventStore.GetEventsAsync(aggregateId);

        // Assert
        var eventList = retrievedEvents.ToList();
        eventList.Should().HaveCount(2);
        eventList[0].Should().BeOfType<NewEntityCreatedEvent>();
        eventList[1].Should().BeOfType<NewEntityNameChangedEvent>();
    }

    [Fact]
    public async Task SaveEventsAsync_WithEmptyEventList_ShouldNotPersistAnything()
    {
        // Arrange
        var aggregateId = new EntityId(Guid.NewGuid());
        var events = new List<IDomainEvent>();

        // Act
        await _eventStore.SaveEventsAsync(aggregateId, events, 0);

        // Assert
        var savedEvents = await _context.Events
            .Where(e => e.AggregateId == aggregateId.Value)
            .ToListAsync();

        savedEvents.Should().BeEmpty();
    }
}
```

#### 4.4 Integration Tests

```csharp
public class NewEntityIntegrationTests : IClassFixture<IntegrationTestFixture>, IDisposable
{
    private readonly IntegrationTestFixture _fixture;
    private readonly IServiceScope _scope;
    private readonly INewEntityRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public NewEntityIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _scope = _fixture.ServiceProvider.CreateScope();
        _repository = _scope.ServiceProvider.GetRequiredService<INewEntityRepository>();
        _unitOfWork = _scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    }

    public void Dispose()
    {
        _scope.Dispose();
    }

    [Fact]
    public async Task CreateNewEntity_ShouldTriggerDomainEventAndNotification()
    {
        // Arrange
        var entity = new NewEntity(new EntityId(Guid.NewGuid()), "Integration Test Entity");

        // Act
        await _repository.SaveAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        // Verify entity is saved
        var saved = await _repository.GetByIdAsync(entity.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Integration Test Entity");

        // Verify domain event was raised
        var events = entity.GetDomainEvents();
        events.Should().ContainSingle(e => e is NewEntityCreatedEvent);

        // Verify SignalR notification was sent (if using test server)
        // This would require setting up SignalR client in test
    }

    [Fact]
    public async Task UpdateEntity_ShouldPersistChangesAndRaiseDomainEvent()
    {
        // Arrange
        var entity = new NewEntity(new EntityId(Guid.NewGuid()), "Original Name");
        await _repository.SaveAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        entity.ClearDomainEvents();

        // Act
        entity.UpdateName("Updated Name");
        await _repository.SaveAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var updated = await _repository.GetByIdAsync(entity.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Name");

        var events = entity.GetDomainEvents();
        events.Should().ContainSingle(e => e is NewEntityNameChangedEvent);
    }

    [Fact]
    public async Task DeleteEntity_ShouldRemoveFromDatabase()
    {
        // Arrange
        var entity = new NewEntity(new EntityId(Guid.NewGuid()), "Test Entity");
        await _repository.SaveAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(entity.Id);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var deleted = await _repository.GetByIdAsync(entity.Id);
        deleted.Should().BeNull();
    }
}

// Test Fixture for Integration Tests
public class IntegrationTestFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; private set; }
    private readonly ServiceCollection _services;

    public IntegrationTestFixture()
    {
        _services = new ServiceCollection();
        ConfigureServices();
        ServiceProvider = _services.BuildServiceProvider();
    }

    private void ConfigureServices()
    {
        // Configure test database
        _services.AddDbContext<FloorballDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Add infrastructure services
        _services.AddScoped<INewEntityRepository, NewEntityRepository>();
        _services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add logging
        _services.AddLogging(builder => builder.AddConsole());

        // Add other required services
        _services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
```

#### 4.5 SignalR Hub Tests

```csharp
public class FloorballNewEntityHubTests
{
    private readonly Mock<ILogger<FloorballNewEntityHub>> _loggerMock;
    private readonly Mock<IHubCallerClients> _clientsMock;
    private readonly Mock<IGroupManager> _groupManagerMock;
    private readonly Mock<HubCallerContext> _contextMock;
    private readonly FloorballNewEntityHub _hub;

    public FloorballNewEntityHubTests()
    {
        _loggerMock = new Mock<ILogger<FloorballNewEntityHub>>();
        _clientsMock = new Mock<IHubCallerClients>();
        _groupManagerMock = new Mock<IGroupManager>();
        _contextMock = new Mock<HubCallerContext>();
        
        _hub = new FloorballNewEntityHub(_loggerMock.Object)
        {
            Clients = _clientsMock.Object,
            Groups = _groupManagerMock.Object,
            Context = _contextMock.Object
        };
    }

    [Fact]
    public async Task JoinNewEntityGroup_ShouldAddToGroup()
    {
        // Arrange
        var connectionId = "test-connection-id";
        var entityId = Guid.NewGuid().ToString();
        
        _contextMock.Setup(x => x.ConnectionId).Returns(connectionId);

        // Act
        await _hub.JoinNewEntityGroup(entityId);

        // Assert
        _groupManagerMock.Verify(
            x => x.AddToGroupAsync(connectionId, $"NewEntity_{entityId}", default),
            Times.Once);
    }

    [Fact]
    public async Task LeaveNewEntityGroup_ShouldRemoveFromGroup()
    {
        // Arrange
        var connectionId = "test-connection-id";
        var entityId = Guid.NewGuid().ToString();
        
        _contextMock.Setup(x => x.ConnectionId).Returns(connectionId);

        // Act
        await _hub.LeaveNewEntityGroup(entityId);

        // Assert
        _groupManagerMock.Verify(
            x => x.RemoveFromGroupAsync(connectionId, $"NewEntity_{entityId}", default),
            Times.Once);
    }
}
```

## 📋 Infrastructure Development Checklist

### Design Phase
- [ ] Database schema designed and reviewed
- [ ] Repository interfaces identified
- [ ] Domain event handlers planned
- [ ] SignalR integration requirements defined
- [ ] Performance and scalability considered

### Implementation Phase
- [ ] Entity configurations implemented
- [ ] Repository implementations completed
- [ ] Domain event handlers implemented
- [ ] SignalR integration completed (if needed)
- [ ] Database migrations created and tested
- [ ] Service registrations updated

### Testing Phase
- [ ] Unit tests for repositories
- [ ] Unit tests for domain event handlers
- [ ] Integration tests for end-to-end scenarios
- [ ] Database migration tests
- [ ] Performance tests for queries
- [ ] SignalR integration tests (if applicable)

### Documentation Phase
- [ ] API documentation updated
- [ ] Database schema documented
- [ ] Configuration changes documented
- [ ] Deployment notes updated

### Quality Phase
- [ ] Code review completed
- [ ] Performance benchmarks run
- [ ] Security review completed
- [ ] Error handling implemented
- [ ] Logging added appropriately

## 🔧 Common Patterns & Examples

### Adding New Sport Infrastructure
1. Create new DbContext for the sport
2. Implement sport-specific repositories
3. Create sport-specific domain event handlers
4. Add SignalR hubs for real-time features
5. Create database migrations
6. Register services in DI container

### Adding External Service Integration
1. Define service interface in Domain
2. Implement concrete service in Infrastructure
3. Add configuration options
4. Implement retry policies and circuit breakers
5. Add health checks
6. Register service with proper lifetime

### Adding Caching Layer
1. Define caching interfaces
2. Implement cache providers (Redis, In-Memory)
3. Add cache-aside pattern to repositories
4. Implement cache invalidation on domain events
5. Add cache configuration options
6. Monitor cache performance

## ⚠️ Common Pitfalls to Avoid

1. **Leaky Abstractions** - Don't expose EF Core types in domain interfaces
2. **N+1 Queries** - Use Include() appropriately and avoid lazy loading issues
3. **Missing Migrations** - Always create migrations for schema changes
4. **Ignoring Performance** - Profile database queries and optimize early
5. **Poor Error Handling** - Handle database exceptions appropriately
6. **Missing Transactions** - Use Unit of Work for data consistency
7. **Hardcoded Configuration** - Externalize all configuration values
8. **Blocking Operations** - Use async/await throughout the stack

## 📊 Performance Considerations

### Database Optimization
- Use appropriate indexes for query patterns
- Implement query splitting for complex includes
- Use compiled queries for frequently executed queries
- Monitor query execution plans
- Implement connection pooling

### SignalR Optimization
- Use groups efficiently to minimize message overhead
- Implement backplane for scale-out scenarios
- Monitor connection counts and message rates
- Use compression for large payloads

### Memory Management
- Dispose DbContext appropriately
- Avoid memory leaks in long-running operations
- Use streaming for large data sets
- Monitor garbage collection patterns

## 🛡️ Security Best Practices

### Database Security
- Use parameterized queries (EF Core handles this)
- Implement row-level security where needed
- Secure connection strings
- Use least privilege principle for database users

### SignalR Security
- Implement authentication and authorization
- Validate all client inputs
- Use HTTPS in production
- Implement rate limiting

## 📚 Additional Resources

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/signalr/)
- [PostgreSQL Best Practices](https://wiki.postgresql.org/wiki/Performance_Optimization)
- [Clean Architecture Guidelines](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS and Event Sourcing](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs) 