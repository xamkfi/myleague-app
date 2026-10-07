---
paths:
  - "tests/**"
---

# Testing

| Layer | Project | What to cover |
|-------|---------|----------------|
| Domain | `tests/backend/Domain.UnitTests/DomainTestProject` | constructors, invariants, state transitions |
| Application | `tests/backend/Application.UnitTests/ApplicationTestProject` | handlers (success + failure), validators, mappers |
| WebAPI | `tests/backend/WebAPI.UnitTests/WebApiTestProject` | controller result mapping, error-to-HTTP mapping, rate limiter |
| Persistence | `tests/backend/Infrastructure.IntegrationTests/InfrastructureIntegrationTestProject` | repositories / EF mappings |
| Importer | `tests/tools/JoomleagueImporter.UnitTests` | Joomleague import mapping |

Stack: xUnit + FluentAssertions 6 + Moq (Moq is not referenced by the Infrastructure or importer test projects). Use explicit types. Name tests `Method_Scenario_Expected`.

Integration tests derive from `Common/BaseIntegrationTest` and run on the **EF InMemory** provider, with one database per test. Testcontainers is referenced but not used. InMemory does not enforce relational constraints or run PostgreSQL SQL, so verify provider-specific behavior against the real database.

```csharp
[Fact]
public async Task Handle_WithNonExistentClub_ShouldReturnFailure()
{
    _clubRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
        .ReturnsAsync((Club?)null);

    Result<ClubDto> result = await _handler.Handle(command, CancellationToken.None);

    result.IsSuccess.Should().BeFalse();
    _teamRepositoryMock.Verify(x => x.AddAsync(It.IsAny<FloorballTeam>()), Times.Never);
}
```

- In Application tests, mock repositories and the unit of work, not the DbContext
- Validator tests: one valid case plus a `[Theory]` for each failing rule
- Do not assert on log text unless the log is the behavior
- When a feature is mirrored across sports, put its tests beside the closest existing floorball/football/hockey suite

Run them: `dotnet test MyLeague.sln`, or narrow it with `dotnet test <project> --filter "FullyQualifiedName~ClassName"`.
