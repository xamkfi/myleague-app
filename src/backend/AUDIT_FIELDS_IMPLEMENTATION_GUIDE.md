# Audit Fields Implementation Guide

This guide explains how to add `Id`, `CreatedAt`, and `UpdatedAt` fields to all entities using the base entity approach with automatic timestamp management in `SaveChanges`.

## Overview

The implementation consists of:
1. **BaseEntity** - Provides common audit fields
2. **AggregateRoot** - Inherits from BaseEntity and adds domain events
3. **DbContextExtensions** - Automatically updates audit fields on save
4. **BaseEntityConfiguration** - Provides common EF Core configuration
5. **Migration Strategy** - How to add fields to existing entities

## Architecture

```
BaseEntity (Id, CreatedAt, UpdatedAt)
    ↓
AggregateRoot (+ Domain Events)
    ↓
Your Entities (Person, Club, FloorballTeam, etc.)
```

## Implementation Steps

### Step 1: Base Classes (✅ Completed)

- `BaseEntity` - Contains audit fields and internal update method
- `AggregateRoot` - Inherits from BaseEntity, adds domain events
- `BaseEntityConfiguration<T>` - Provides common EF configuration

### Step 2: Update DbContextExtensions (✅ Completed)

The `SaveChangesWithEventsAsync` method now:
1. Updates audit fields automatically
2. Processes domain events
3. Saves to database
4. Dispatches events

### Step 3: Update Existing Entities

For each entity, follow this pattern:

#### Before:
```csharp
public class Person : AggregateRoot
{
    public Guid Id { get; private set; } // Remove this
    
    protected Person()
    {
        Id = Guid.NewGuid(); // Remove this
        // ...
    }
    
    public Person(/* parameters */)
    {
        Id = Guid.NewGuid(); // Remove this
        // ...
    }
}
```

#### After:
```csharp
public class Person : AggregateRoot
{
    // Id, CreatedAt, UpdatedAt are inherited from BaseEntity
    
    protected Person()
    {
        // BaseEntity constructor handles Id generation
        // ...
    }
    
    public Person(/* parameters */)
    {
        // BaseEntity constructor handles Id generation
        // ...
    }
}
```

### Step 4: Update Entity Configurations

#### Option A: Use BaseEntityConfiguration (Recommended)
```csharp
public class PersonConfiguration : BaseEntityConfiguration<Person>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Person> builder)
    {
        // Configure only Person-specific properties
        builder.Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(100);
        // ...
    }
}
```

#### Option B: Manual Configuration
```csharp
public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        // Configure audit fields manually
        builder.HasKey(p => p.Id);
        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");
        // ... etc
    }
}
```

### Step 5: Create Database Migrations

For each DbContext, create migrations to add audit fields:

```bash
# For CommonDbContext
dotnet ef migrations add AddAuditFieldsToCommonEntities --context CommonDbContext

# For FloorballDbContext  
dotnet ef migrations add AddAuditFieldsToFloorballEntities --context FloorballDbContext

# For HockeyDbContext
dotnet ef migrations add AddAuditFieldsToHockeyEntities --context HockeyDbContext
```

## Entity Update Checklist

For each entity, complete these tasks:

### ✅ Person (Completed)
- [x] Remove manual `Id` property
- [x] Remove `Id = Guid.NewGuid()` from constructors
- [x] Update configuration to use `BaseEntityConfiguration<Person>`
- [ ] Create migration to add audit fields
- [ ] Test entity creation and updates

### 🔄 Club (Next)
- [ ] Remove manual `Id` property
- [ ] Remove `Id = Guid.NewGuid()` from constructors  
- [ ] Update configuration to use `BaseEntityConfiguration<Club>`
- [ ] Create migration to add audit fields
- [ ] Test entity creation and updates

### 🔄 FloorballTeam
- [ ] Remove manual `Id` property
- [ ] Remove `Id = Guid.NewGuid()` from constructors
- [ ] Update configuration to use `BaseEntityConfiguration<FloorballTeam>`
- [ ] Create migration to add audit fields
- [ ] Test entity creation and updates

### 🔄 FloorballPlayer
- [ ] Remove manual `Id` property
- [ ] Remove `Id = Guid.NewGuid()` from constructors
- [ ] Update configuration to use `BaseEntityConfiguration<FloorballPlayer>`
- [ ] Create migration to add audit fields
- [ ] Test entity creation and updates

### 🔄 Other Entities
Apply the same pattern to:
- [ ] FloorballSeason
- [ ] FloorballMatch
- [ ] FloorballReferee
- [ ] FloorballCoach
- [ ] FloorballTeamManager
- [ ] HockeyTeam
- [ ] HockeyPlayer
- [ ] HockeySeason
- [ ] HockeyMatch
- [ ] HockeyReferee
- [ ] Division
- [ ] NewsArticle (already has audit fields)

## Benefits

### 1. Automatic Timestamp Management
```csharp
// Before - Manual timestamp management
public void UpdateBasicInfo(string firstName, string lastName)
{
    FirstName = firstName;
    LastName = lastName;
    UpdatedAt = DateTime.UtcNow; // Manual
}

// After - Automatic timestamp management
public void UpdateBasicInfo(string firstName, string lastName)
{
    FirstName = firstName;
    LastName = lastName;
    // UpdatedAt automatically set in SaveChanges
}
```

### 2. Consistent Audit Fields
All entities automatically get:
- `Id` (Guid, generated in constructor)
- `CreatedAt` (DateTime, set on creation)
- `UpdatedAt` (DateTime?, set on modification)

### 3. Optimized Database Queries
Automatic indexes on audit fields:
- `IX_{EntityName}_CreatedAt` - For chronological queries
- `IX_{EntityName}_UpdatedAt` - For finding recent changes
- `IX_{EntityName}_Audit` - Composite index for audit queries

### 4. Clean Domain Logic
Business methods focus on business rules, not infrastructure concerns.

## Usage Examples

### Creating Entities
```csharp
var person = new Person("John", "Doe", DateTime.Parse("1990-01-01"));
// person.Id is automatically generated
// person.CreatedAt is automatically set
// person.UpdatedAt is null

await unitOfWork.SaveChangesAsync();
// Audit fields are handled automatically
```

### Updating Entities
```csharp
person.UpdateBasicInfo("Jane", "Smith");
// person.UpdatedAt will be automatically set when SaveChanges is called

await unitOfWork.SaveChangesAsync();
// person.UpdatedAt is now set to current UTC time
```

### Querying with Audit Fields
```csharp
// Find recently created persons
var recentPersons = await context.Persons
    .Where(p => p.CreatedAt >= DateTime.UtcNow.AddDays(-7))
    .OrderByDescending(p => p.CreatedAt)
    .ToListAsync();

// Find recently updated persons
var updatedPersons = await context.Persons
    .Where(p => p.UpdatedAt.HasValue && p.UpdatedAt >= DateTime.UtcNow.AddHours(-1))
    .OrderByDescending(p => p.UpdatedAt)
    .ToListAsync();
```

## Testing

### Unit Tests
```csharp
[Test]
public void Person_Creation_SetsAuditFields()
{
    // Arrange & Act
    var person = new Person("John", "Doe", DateTime.Parse("1990-01-01"));
    
    // Assert
    Assert.That(person.Id, Is.Not.EqualTo(Guid.Empty));
    Assert.That(person.CreatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
    Assert.That(person.UpdatedAt, Is.Null);
}

[Test]
public async Task Person_Update_SetsUpdatedAt()
{
    // Arrange
    var person = new Person("John", "Doe", DateTime.Parse("1990-01-01"));
    context.Persons.Add(person);
    await context.SaveChangesAsync();
    
    var originalUpdatedAt = person.UpdatedAt;
    
    // Act
    person.UpdateBasicInfo("Jane", "Smith");
    await context.SaveChangesAsync();
    
    // Assert
    Assert.That(person.UpdatedAt, Is.Not.EqualTo(originalUpdatedAt));
    Assert.That(person.UpdatedAt, Is.Not.Null);
}
```

## Migration Commands

```bash
# Generate migrations for each context
dotnet ef migrations add AddAuditFields --context CommonDbContext --output-dir Migrations/CommonDb
dotnet ef migrations add AddAuditFields --context FloorballDbContext --output-dir Migrations/FloorballDb  
dotnet ef migrations add AddAuditFields --context HockeyDbContext --output-dir Migrations/HockeyDb

# Apply migrations
dotnet ef database update --context CommonDbContext
dotnet ef database update --context FloorballDbContext
dotnet ef database update --context HockeyDbContext
```

## Next Steps

1. **Update Club Entity** - Apply the same pattern as Person
2. **Update FloorballTeam Entity** - Remove manual ID management
3. **Create Migrations** - Add audit fields to database tables
4. **Update Remaining Entities** - Follow the checklist above
5. **Add Integration Tests** - Verify audit fields work end-to-end
6. **Update API Documentation** - Include audit fields in API responses

## Notes

- The `NewsArticle` entity already has audit fields and can serve as a reference
- Event-sourced entities (`EventSourcedFloorballMatch`) may need special consideration
- Consider adding audit field validation in integration tests
- Monitor database performance after adding indexes 