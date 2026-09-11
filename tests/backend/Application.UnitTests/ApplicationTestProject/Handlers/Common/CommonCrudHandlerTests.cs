using Application.Common;
using Application.Features.Common.Divisions.Commands;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.Divisions.Handlers;
using Application.Features.Common.Divisions.Queries;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.News.Handlers;
using Application.Features.Common.News.Queries;
using Application.Features.Common.Persons.Commands;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Persons.Handlers;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Users.Handlers;
using Application.Features.Common.Users.Queries;
using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Common;

public class CommonCrudHandlerTests
{
    [Fact]
    public async Task CreatePerson_WhenNameExists_ReturnsFailure()
    {
        Mock<IPersonRepository> repo = new();
        CreatePersonHandler handler = new(
            repo.Object,
            Mock.Of<IUnitOfWork>(),
            Mock.Of<ILogger<CreatePersonHandler>>());
        repo.Setup(r => r.ExistsByFullNameAsync("Ada", "Lovelace")).ReturnsAsync(true);

        Result<PersonDto> result = await handler.Handle(
            new CreatePersonCommand("Ada", "Lovelace"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");
        repo.Verify(r => r.AddAsync(It.IsAny<Person>()), Times.Never);
    }

    [Fact]
    public async Task CreatePerson_Valid_AddsAndSaves()
    {
        Mock<IPersonRepository> repo = new();
        Mock<IUnitOfWork> uow = new();
        CreatePersonHandler handler = new(
            repo.Object,
            uow.Object,
            Mock.Of<ILogger<CreatePersonHandler>>());
        repo.Setup(r => r.ExistsByFullNameAsync("Ada", "Lovelace")).ReturnsAsync(false);

        Result<PersonDto> result = await handler.Handle(
            new CreatePersonCommand("Ada", "Lovelace"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.FirstName.Should().Be("Ada");
        repo.Verify(r => r.AddAsync(It.IsAny<Person>()), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateDivision_WhenDuplicate_ReturnsFailure()
    {
        Mock<IDivisionRepository> repo = new();
        CreateDivisionHandler handler = new(
            repo.Object,
            Mock.Of<IUnitOfWork>(),
            Mock.Of<ILogger<CreateDivisionHandler>>());
        repo.Setup(r => r.ExistsAsync("Championship", SportsCategory.Floorball)).ReturnsAsync(true);

        Result<DivisionDto> result = await handler.Handle(
            new CreateDivisionCommand("Championship", "Desc", 1, SportsCategory.Floorball),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateDivision_Valid_AddsAndSaves()
    {
        Mock<IDivisionRepository> repo = new();
        Mock<IUnitOfWork> uow = new();
        CreateDivisionHandler handler = new(
            repo.Object,
            uow.Object,
            Mock.Of<ILogger<CreateDivisionHandler>>());
        repo.Setup(r => r.ExistsAsync("Championship", SportsCategory.Floorball)).ReturnsAsync(false);

        Result<DivisionDto> result = await handler.Handle(
            new CreateDivisionCommand("Championship", "Desc", 1, SportsCategory.Floorball),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repo.Verify(r => r.AddAsync(It.IsAny<Division>()), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDivisionById_WhenMissing_ReturnsNotFound()
    {
        Mock<IDivisionRepository> repo = new();
        GetDivisionByIdHandler handler = new(repo.Object, Mock.Of<ILogger<GetDivisionByIdHandler>>());
        Guid id = Guid.NewGuid();
        repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Division?)null);

        Result<DivisionDto> result = await handler.Handle(new GetDivisionByIdQuery(id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Division");
    }

    [Fact]
    public async Task GetUserById_WhenMissing_ReturnsNotFound()
    {
        Mock<IUserRepository> repo = new();
        GetUserByIdHandler handler = new(repo.Object, Mock.Of<ILogger<GetUserByIdHandler>>());
        Guid id = Guid.NewGuid();
        repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((User?)null);

        Result<UserDto> result = await handler.Handle(new GetUserByIdQuery(id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("User");
    }

    [Fact]
    public async Task GetNewsArticleById_WhenMissing_ReturnsNotFound()
    {
        Mock<INewsArticleRepository> repo = new();
        GetNewsArticleByIdHandler handler = new(repo.Object, Mock.Of<ILogger<GetNewsArticleByIdHandler>>());
        Guid id = Guid.NewGuid();
        repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((NewsArticle?)null);

        Result<NewsArticleDto> result = await handler.Handle(
            new GetNewsArticleByIdQuery(id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("NewsArticle");
    }
}
