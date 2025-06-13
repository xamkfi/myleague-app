using Application.Queries.NewsArticles;
using Application.Services.Common;
using Application.Validators.Queries.NewsArticles;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace ApplicationTestProject.Validators.Queries.NewsArticles;

public class GetAllNewsArticlesQueryValidatorTests
{
    private readonly Mock<IPaginationService> _mockPaginationService;
    private readonly GetAllNewsArticlesQueryValidator _validator;

    public GetAllNewsArticlesQueryValidatorTests()
    {
        _mockPaginationService = new Mock<IPaginationService>();
        
        // Remove the generic setup that conflicts with individual test mocks
        // Each test will set up its own specific mock expectations
        _mockPaginationService.Setup(x => x.GetPaginationSettings("News"))
            .Returns(new PaginationSettings(10, 50, 1));
            
        _validator = new GetAllNewsArticlesQueryValidator(_mockPaginationService.Object);
    }

    [Fact]
    public void Validate_ValidQuery_ShouldNotHaveValidationErrors()
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(
            Page: 1,
            PageSize: 10,
            Category: "General",
            SportCategory: "Football",
            Author: "Test Author",
            IncludeArchived: false
        );

        // Setup mock to return true for valid PageSize (10)
        _mockPaginationService.Setup(x => x.IsValidPageSize("News", 10))
            .Returns(true);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidQueryWithDefaults_ShouldNotHaveValidationErrors()
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery();
        
        // Setup mock to return true for default PageSize (0)
        _mockPaginationService.Setup(x => x.IsValidPageSize("News", 0))
            .Returns(true);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_InvalidPageNumber_ShouldHaveValidationError(int invalidPage)
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(Page: invalidPage);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Page)
            .WithErrorMessage("Page must be greater than 0");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_ValidPageNumber_ShouldNotHaveValidationError(int validPage)
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(Page: validPage);

        // Setup mock to return true for default PageSize (0) since we're only testing Page
        _mockPaginationService.Setup(x => x.IsValidPageSize("News", 0))
            .Returns(true);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(51)]  // Above News max of 50
    [InlineData(1000)]
    public void Validate_InvalidPageSize_ShouldHaveValidationError(int invalidPageSize)
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(PageSize: invalidPageSize);
        
        // Setup mock to return false for invalid page sizes
        _mockPaginationService.Setup(x => x.IsValidPageSize("News", invalidPageSize))
            .Returns(false);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData(0)]   // Special case - should be valid (means use default)
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]  // News max is 50
    public void Validate_ValidPageSize_ShouldNotHaveValidationError(int validPageSize)
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(PageSize: validPageSize);
        
        // Setup mock to return true for valid page sizes
        _mockPaginationService.Setup(x => x.IsValidPageSize("News", validPageSize))
            .Returns(true);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData("General")]
    [InlineData("MatchReports")]
    [InlineData("Transfers")]
    [InlineData("PlayerUpdates")]
    [InlineData("TeamNews")]
    public void Validate_ValidCategory_ShouldNotHaveValidationError(string? category)
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(Category: category);

        // Setup mock to return true for default PageSize (0) since we're only testing Category
        _mockPaginationService.Setup(x => x.IsValidPageSize("News", 0))
            .Returns(true);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Category);
    }

    [Theory]
    [InlineData("InvalidCategory")]
    [InlineData("Random")]
    public void Validate_InvalidCategory_ShouldHaveValidationError(string invalidCategory)
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(Category: invalidCategory);

        // Setup mock to return true for default PageSize (0) since we're only testing Category
        _mockPaginationService.Setup(x => x.IsValidPageSize("News", 0))
            .Returns(true);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage("Invalid news category");
    }

    [Theory]
    [InlineData("Football")]
    [InlineData("Icehockey")]
    [InlineData("Floorball")]
    public void Validate_ValidSportCategory_ShouldNotHaveValidationError(string? sportCategory)
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(SportCategory: sportCategory);

        // Setup mock to return true for default PageSize (0) since we're only testing SportCategory
        _mockPaginationService.Setup(x => x.IsValidPageSize("News", 0))
            .Returns(true);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SportCategory);
    }

    [Theory]
    [InlineData("InvalidSport")]
    [InlineData("Soccer")]
    public void Validate_InvalidSportCategory_ShouldHaveValidationError(string invalidSportCategory)
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(SportCategory: invalidSportCategory);

        // Setup mock to return true for default PageSize (0) since we're only testing SportCategory
        _mockPaginationService.Setup(x => x.IsValidPageSize("News", 0))
            .Returns(true);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SportCategory)
            .WithErrorMessage("Invalid sport category");
    }

    [Fact]
    public void Validate_AuthorTooLong_ShouldHaveValidationError()
    {
        // Arrange
        string longAuthor = new string('A', 101); // 101 characters
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(Author: longAuthor);

        // Setup mock to return true for default PageSize (0) since we're only testing Author
        _mockPaginationService.Setup(x => x.IsValidPageSize("News", 0))
            .Returns(true);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Author)
            .WithErrorMessage("Author filter cannot exceed 100 characters");
    }

    [Theory]
    [InlineData("Valid Author")]
    [InlineData("A")]
    [InlineData(null)]
    public void Validate_ValidAuthor_ShouldNotHaveValidationError(string? author)
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(Author: author);

        // Setup mock to return true for default PageSize (0) since we're only testing Author
        _mockPaginationService.Setup(x => x.IsValidPageSize("News", 0))
            .Returns(true);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Author);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_ValidIncludeArchived_ShouldNotHaveValidationError(bool includeArchived)
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(IncludeArchived: includeArchived);

        // Setup mock to return true for default PageSize (0) since we're only testing IncludeArchived
        _mockPaginationService.Setup(x => x.IsValidPageSize("News", 0))
            .Returns(true);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IncludeArchived);
    }

    [Fact]
    public void Validate_MultipleValidationErrors_ShouldHaveAllErrors()
    {
        // Arrange
        string longAuthor = new string('A', 101);
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(
            Page: 0,                    // Invalid page
            PageSize: 101,              // Invalid page size
            Category: "InvalidCategory", // Invalid category
            SportCategory: "InvalidSport", // Invalid sport category
            Author: longAuthor          // Invalid author length
        );

        // Setup mock to return false for invalid PageSize (101)
        _mockPaginationService.Setup(x => x.IsValidPageSize("News", 101))
            .Returns(false);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Page);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
        result.ShouldHaveValidationErrorFor(x => x.Category);
        result.ShouldHaveValidationErrorFor(x => x.SportCategory);
        result.ShouldHaveValidationErrorFor(x => x.Author);
    }

    [Fact]
    public void Validate_EdgeCaseValues_ShouldHandleCorrectly()
    {
        // Arrange
        string maxLengthAuthor = new string('A', 100); // Exactly 100 characters
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(
            Page: 1,
            PageSize: 50,              // Changed from 100 to 50 (News max)
            Category: "General",
            SportCategory: "Football",
            Author: maxLengthAuthor,
            IncludeArchived: false
        );
        
        // Setup mock to return true for valid PageSize (50)
        _mockPaginationService.Setup(x => x.IsValidPageSize("News", 50))
            .Returns(true);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
} 