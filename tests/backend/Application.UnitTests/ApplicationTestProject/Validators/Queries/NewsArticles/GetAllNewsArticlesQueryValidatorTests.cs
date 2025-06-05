using Application.Queries.NewsArticles;
using Application.Validators.Queries.NewsArticles;
using FluentValidation.TestHelper;
using Xunit;

namespace ApplicationTestProject.Validators.Queries.NewsArticles;

public class GetAllNewsArticlesQueryValidatorTests
{
    private readonly GetAllNewsArticlesQueryValidator _validator;

    public GetAllNewsArticlesQueryValidatorTests()
    {
        _validator = new GetAllNewsArticlesQueryValidator();
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

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(1000)]
    public void Validate_InvalidPageSize_ShouldHaveValidationError(int invalidPageSize)
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(PageSize: invalidPageSize);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("Page size must be between 1 and 100");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_ValidPageSize_ShouldNotHaveValidationError(int validPageSize)
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(PageSize: validPageSize);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData("General")]
    [InlineData("MatchReports")]
    [InlineData("Transfers")]
    [InlineData("PlayerNews")]
    [InlineData("TeamNews")]
    public void Validate_ValidCategory_ShouldNotHaveValidationError(string? category)
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(Category: category);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Category);
    }

    [Theory]
    [InlineData("InvalidCategory")]
    [InlineData("Random")]
    [InlineData("")]
    public void Validate_InvalidCategory_ShouldHaveValidationError(string invalidCategory)
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(Category: invalidCategory);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage("Category must be a valid news category");
    }

    [Theory]
    [InlineData("Football")]
    [InlineData("Icehockey")]
    [InlineData("Basketball")]
    [InlineData("Handball")]
    [InlineData("Volleyball")]
    public void Validate_ValidSportCategory_ShouldNotHaveValidationError(string? sportCategory)
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(SportCategory: sportCategory);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SportCategory);
    }

    [Theory]
    [InlineData("InvalidSport")]
    [InlineData("Soccer")]
    [InlineData("")]
    public void Validate_InvalidSportCategory_ShouldHaveValidationError(string invalidSportCategory)
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(SportCategory: invalidSportCategory);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SportCategory)
            .WithErrorMessage("Sport category must be a valid sport category");
    }

    [Fact]
    public void Validate_AuthorTooLong_ShouldHaveValidationError()
    {
        // Arrange
        string longAuthor = new string('A', 101); // 101 characters
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(Author: longAuthor);

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Author)
            .WithErrorMessage("Author cannot exceed 100 characters");
    }

    [Theory]
    [InlineData("Valid Author")]
    [InlineData("A")]
    [InlineData(null)]
    public void Validate_ValidAuthor_ShouldNotHaveValidationError(string? author)
    {
        // Arrange
        GetAllNewsArticlesQuery query = new GetAllNewsArticlesQuery(Author: author);

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
            PageSize: 100,
            Category: "General",
            SportCategory: "Football",
            Author: maxLengthAuthor,
            IncludeArchived: false
        );

        // Act
        TestValidationResult<GetAllNewsArticlesQuery> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
} 