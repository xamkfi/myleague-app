# Feature Development Guide - Domain Layer

## Overview

This guide provides step-by-step instructions for developing new features in the Domain layer while maintaining Domain-Driven Design (DDD) principles, code quality, and architectural consistency.

## 🎯 Before You Start

### Prerequisites
- Understanding of Domain-Driven Design (DDD) principles
- Familiarity with CQRS and Event Sourcing patterns
- Knowledge of the existing domain model and ubiquitous language
- Review of `DomainGlossary.md` for current terminology

### Key Principles to Follow
1. **Ubiquitous Language** - Use domain expert terminology consistently
2. **Aggregate Boundaries** - Maintain clear aggregate boundaries
3. **Domain Events** - Capture all significant business events
4. **Immutability** - Value objects must be immutable
5. **Business Logic** - Keep business rules in the domain layer

## 🚀 Development Process

### Step 1: Domain Analysis & Design

#### 1.1 Understand the Business Requirement
- [ ] Meet with domain experts to understand the requirement
- [ ] Identify the business problem being solved
- [ ] Document the acceptance criteria
- [ ] Understand the business rules and constraints

#### 1.2 Domain Modeling
- [ ] Identify if this is a new aggregate or extends existing ones
- [ ] Determine required entities, value objects, and enums
- [ ] Define domain events that should be raised
- [ ] Map out aggregate boundaries and relationships
- [ ] Update the ubiquitous language if needed

#### 1.3 Design Decisions
- [ ] Choose between traditional aggregates vs event-sourced aggregates
- [ ] Determine repository interfaces needed
- [ ] Plan integration points with other bounded contexts

### Step 2: Implementation

#### 2.1 Update Domain Glossary
```markdown
## New Term
Brief description of the new business concept, its purpose, and relationships.
```

#### 2.2 Create/Update Entities

**For New Entities:**
```csharp
namespace Domain.Entities.[Sport/Common]
{
    public class NewEntity : AggregateRoot<EntityId>
    {
        // Private fields for encapsulation
        private readonly List<DomainEvent> _domainEvents = new();
        
        // Public properties (readonly when possible)
        public string Name { get; private set; }
        
        // Constructor(s)
        private NewEntity() { } // For EF Core
        
        public NewEntity(EntityId id, string name)
        {
            Id = id;
            Name = name;
            
            // Raise domain event
            RaiseDomainEvent(new EntityCreatedEvent(id, name));
        }
        
        // Business methods
        public void UpdateName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new DomainException("Name cannot be empty");
                
            var oldName = Name;
            Name = newName;
            
            RaiseDomainEvent(new EntityNameChangedEvent(Id, oldName, newName));
        }
        
        // Domain events
        protected override void RaiseDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
        
        public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
```

#### 2.3 Create Value Objects
```csharp
namespace Domain.ValueObjects.[Sport/Common]
{
    public record NewValueObject
    {
        public string Property1 { get; init; }
        public int Property2 { get; init; }
        
        public NewValueObject(string property1, int property2)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(property1))
                throw new ArgumentException("Property1 cannot be empty");
                
            if (property2 < 0)
                throw new ArgumentException("Property2 must be positive");
                
            Property1 = property1;
            Property2 = property2;
        }
    }
}
```

#### 2.4 Create Enumerations
```csharp
namespace Domain.Enums.[Sport]
{
    public enum NewEnum
    {
        Value1 = 1,
        Value2 = 2,
        Value3 = 3
    }
}
```

#### 2.5 Define Domain Events
```csharp
namespace Domain.DomainEvents.[Sport/Common]
{
    public record NewDomainEvent(
        EntityId EntityId,
        string Property,
        DateTime OccurredAt = default
    ) : IDomainEvent
    {
        public DateTime OccurredAt { get; init; } = OccurredAt == default ? DateTime.UtcNow : OccurredAt;
    }
}
```

#### 2.6 Create Repository Interfaces
```csharp
namespace Domain.Repositories.[Sport/Common]
{
    public interface INewEntityRepository
    {
        Task<NewEntity?> GetByIdAsync(EntityId id, CancellationToken cancellationToken = default);
        Task<IEnumerable<NewEntity>> GetByConditionAsync(/* parameters */, CancellationToken cancellationToken = default);
        Task SaveAsync(NewEntity entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(EntityId id, CancellationToken cancellationToken = default);
    }
}
```

#### 2.7 Event Sourcing (If Applicable)
```csharp
namespace Domain.Entities.[Sport]
{
    public class EventSourcedNewEntity : EventSourcedAggregate<EntityId>
    {
        // State properties
        public string Name { get; private set; } = string.Empty;
        
        // Constructor for reconstruction
        public EventSourcedNewEntity(EntityId id, IEnumerable<IDomainEvent> events) : base(id)
        {
            foreach (var @event in events)
            {
                Apply(@event);
            }
        }
        
        // Business method
        public void ChangeName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new DomainException("Name cannot be empty");
                
            var @event = new EntityNameChangedEvent(Id, Name, newName);
            ApplyAndRaise(@event);
        }
        
        // Event application
        protected override void Apply(IDomainEvent @event)
        {
            switch (@event)
            {
                case EntityCreatedEvent created:
                    Name = created.Name;
                    break;
                case EntityNameChangedEvent nameChanged:
                    Name = nameChanged.NewName;
                    break;
            }
        }
    }
}
```

### Step 3: Testing

#### 3.1 Unit Tests for Entities
```csharp
[Test]
public void Constructor_WithValidParameters_ShouldCreateEntity()
{
    // Arrange
    var id = new EntityId(Guid.NewGuid());
    var name = "Test Name";
    
    // Act
    var entity = new NewEntity(id, name);
    
    // Assert
    entity.Id.Should().Be(id);
    entity.Name.Should().Be(name);
    entity.GetDomainEvents().Should().ContainSingle()
        .Which.Should().BeOfType<EntityCreatedEvent>();
}

[Test]
public void UpdateName_WithValidName_ShouldUpdateAndRaiseDomainEvent()
{
    // Arrange
    var entity = new NewEntity(new EntityId(Guid.NewGuid()), "Original");
    entity.ClearDomainEvents();
    
    // Act
    entity.UpdateName("Updated");
    
    // Assert
    entity.Name.Should().Be("Updated");
    entity.GetDomainEvents().Should().ContainSingle()
        .Which.Should().BeOfType<EntityNameChangedEvent>();
}
```

#### 3.2 Unit Tests for Value Objects
```csharp
[Test]
public void Constructor_WithValidParameters_ShouldCreateValueObject()
{
    // Arrange & Act
    var valueObject = new NewValueObject("test", 5);
    
    // Assert
    valueObject.Property1.Should().Be("test");
    valueObject.Property2.Should().Be(5);
}

[Test]
public void Constructor_WithInvalidParameter_ShouldThrowException()
{
    // Act & Assert
    var act = () => new NewValueObject("", 5);
    act.Should().Throw<ArgumentException>();
}
```

### Step 4: Documentation Updates

#### 4.1 Update Domain Glossary
- [ ] Add new terms to `DomainGlossary.md`
- [ ] Update existing terms if modified
- [ ] Include relationships and business rules

#### 4.2 Update README (if needed)
- [ ] Add new aggregate roots to the list
- [ ] Update project structure if new folders were added
- [ ] Document any new patterns or conventions

## 📋 Feature Development Checklist

### Design Phase
- [ ] Business requirement understood and documented
- [ ] Domain model designed with clear aggregate boundaries
- [ ] Ubiquitous language terms identified
- [ ] Domain events identified
- [ ] Repository interfaces planned

### Implementation Phase
- [ ] Domain glossary updated
- [ ] Entities implemented with proper encapsulation
- [ ] Value objects are immutable and validated
- [ ] Domain events defined and raised appropriately
- [ ] Repository interfaces created
- [ ] Event sourcing implemented (if applicable)

### Testing Phase
- [ ] Unit tests for all entities (positive and negative cases)
- [ ] Unit tests for all value objects
- [ ] Unit tests for domain events
- [ ] Integration tests for complex business scenarios
- [ ] All tests passing

### Documentation Phase
- [ ] Domain glossary updated
- [ ] Code comments added for complex business rules
- [ ] README updated if necessary
- [ ] Architecture documentation updated

### Code Quality
- [ ] No compiler warnings
- [ ] Code analysis warnings resolved
- [ ] Consistent naming conventions followed
- [ ] Proper exception handling implemented
- [ ] Business rules enforced in domain layer

## 🔧 Common Patterns & Examples

### Adding a New Sport
1. Create new folders under each domain folder (Entities, ValueObjects, etc.)
2. Implement sport-specific entities inheriting from common base classes
3. Define sport-specific enums and value objects
4. Create sport-specific domain events
5. Implement repository interfaces
6. Update domain glossary with sport-specific terminology

### Extending Existing Aggregates
1. Add new properties with proper validation
2. Create new domain events for state changes
3. Update existing business methods if needed
4. Maintain backward compatibility
5. Add comprehensive tests

### Adding Cross-Aggregate Features
1. Use domain events for communication between aggregates
2. Avoid direct references between aggregates
3. Consider eventual consistency
4. Implement proper transaction boundaries

## ⚠️ Common Pitfalls to Avoid

1. **Anemic Domain Model** - Don't just create data containers; include business logic
2. **Large Aggregates** - Keep aggregates focused and bounded
3. **Missing Domain Events** - Capture all significant business events
4. **Tight Coupling** - Avoid dependencies between aggregates
5. **Business Logic in Services** - Keep core business rules in the domain
6. **Mutable Value Objects** - Value objects must be immutable
7. **Poor Exception Handling** - Use domain-specific exceptions
8. **Ignoring Ubiquitous Language** - Always use domain expert terminology

## 📚 Additional Resources

- [Domain-Driven Design Reference](https://www.domainlanguage.com/ddd/reference/)
- [Event Sourcing Patterns](https://docs.microsoft.com/en-us/azure/architecture/patterns/event-sourcing)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- Domain Glossary (`DomainGlossary.md`)
- Project README (`README.md`) 