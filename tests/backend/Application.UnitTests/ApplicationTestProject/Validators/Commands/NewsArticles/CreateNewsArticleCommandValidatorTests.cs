using Application.Commands.NewsArticles;
using Application.Validators.Commands.NewsArticles;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using Xunit;

namespace ApplicationTestProject.Validators.Commands.NewsArticles;

public class CreateNewsArticleCommandValidatorTests
{
    private readonly CreateNewsArticleCommandValidator _validator;

    public CreateNewsArticleCommandValidatorTests()
    {
        _validator = new CreateNewsArticleCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Valid News Article Title",
            "<p>Valid HTML content</p>",
            "Valid summary text",
            new List<string> { "https://example.com/image.jpg" },
            "Valid Author",
            "General",
            "Football",
            new List<string> { "tag1", "tag2" }
        );

        // Act
        TestValidationResult<CreateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidCommandWithMinimalData_ShouldNotHaveValidationErrors()
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Valid Title",
            "<p>Valid content</p>"
        );

        // Act
        TestValidationResult<CreateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyOrNullTitle_ShouldHaveValidationError(string? title)
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            title!,
            "<p>Valid content</p>"
        );

        // Act
        TestValidationResult<CreateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public void Validate_TitleTooLong_ShouldHaveValidationError()
    {
        // Arrange
        string longTitle = new string('A', 201); // 201 characters
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            longTitle,
            "<p>Valid content</p>"
        );

        // Act
        TestValidationResult<CreateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title cannot exceed 200 characters");
    }

    [Fact]
    public void Validate_TitleExactlyMaxLength_ShouldNotHaveValidationError()
    {
        // Arrange
        string maxLengthTitle = new string('A', 200); // Exactly 200 characters
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            maxLengthTitle,
            "<p>Valid content</p>"
        );

        // Act
        TestValidationResult<CreateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyOrNullContent_ShouldHaveValidationError(string? content)
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Valid Title",
            content!
        );

        // Act
        TestValidationResult<CreateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ContentHtml)
            .WithErrorMessage("Content is required");
    }

    [Fact]
    public void Validate_SummaryTooLong_ShouldHaveValidationError()
    {
        // Arrange
        string longSummary = new string('B', 501); // 501 characters
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Valid Title",
            "<p>Valid content</p>",
            longSummary
        );

        // Act
        TestValidationResult<CreateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Summary)
            .WithErrorMessage("Summary cannot exceed 500 characters");
    }

    [Fact]
    public void Validate_SummaryExactlyMaxLength_ShouldNotHaveValidationError()
    {
        // Arrange
        string maxLengthSummary = new string('B', 500); // Exactly 500 characters
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Valid Title",
            "<p>Valid content</p>",
            maxLengthSummary
        );

        // Act
        TestValidationResult<CreateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Summary);
    }

    [Fact]
    public void Validate_AuthorTooLong_ShouldHaveValidationError()
    {
        // Arrange
        string longAuthor = new string('C', 101); // 101 characters
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Valid Title",
            "<p>Valid content</p>",
            null,
            null,
            longAuthor
        );

        // Act
        TestValidationResult<CreateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Author)
            .WithErrorMessage("Author name cannot exceed 100 characters");
    }

    [Theory]
    [InlineData("General")]
    [InlineData("MatchReports")]
    [InlineData("Transfers")]
    [InlineData("PlayerUpdates")]
    [InlineData("TeamNews")]
    public void Validate_ValidCategory_ShouldNotHaveValidationError(string category)
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Valid Title",
            "<p>Valid content</p>",
            null,
            null,
            null,
            category
        );

        // Act
        TestValidationResult<CreateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Category);
    }

    [Theory]
    [InlineData("InvalidCategory")]
    [InlineData("Random")]
    public void Validate_InvalidCategory_ShouldHaveValidationError(string invalidCategory)
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Valid Title",
            "<p>Valid content</p>",
            null,
            null,
            null,
            invalidCategory
        );

        // Act
        TestValidationResult<CreateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage("Invalid news category");
    }

    [Theory]
    [InlineData("Football")]
    [InlineData("Icehockey")]
    [InlineData("Floorball")]
    public void Validate_ValidSportCategory_ShouldNotHaveValidationError(string sportCategory)
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Valid Title",
            "<p>Valid content</p>",
            null,
            null,
            null,
            null,
            sportCategory
        );

        // Act
        TestValidationResult<CreateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SportCategory);
    }

    [Theory]
    [InlineData("InvalidSport")]
    [InlineData("Soccer")]
    public void Validate_InvalidSportCategory_ShouldHaveValidationError(string invalidSportCategory)
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Valid Title",
            "<p>Valid content</p>",
            null,
            null,
            null,
            null,
            invalidSportCategory
        );

        // Act
        TestValidationResult<CreateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SportCategory)
            .WithErrorMessage("Invalid sport category");
    }

    [Theory]
    [InlineData("https://example.com/image.jpg")]
    [InlineData("http://example.com/image.png")]
    [InlineData("https://cdn.example.com/images/photo.gif")]
    public void Validate_ValidImageUrls_ShouldNotHaveValidationError(string imageUrl)
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Valid Title",
            "<p>Valid content</p>",
            null,
            new List<string> { imageUrl }
        );

        // Act
        TestValidationResult<CreateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ImageUrls);
    }

    [Theory]
    [InlineData("invalid-url")]
    [InlineData("ftp://example.com/image.jpg")]
    [InlineData("not-a-url")]
    public void Validate_InvalidImageUrls_ShouldHaveValidationError(string invalidUrl)
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Valid Title",
            "<p>Valid content</p>",
            null,
            new List<string> { invalidUrl }
        );

        // Act
        TestValidationResult<CreateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ImageUrls)
            .WithErrorMessage("Invalid image URL format");
    }

    [Fact]
    public void Validate_DuplicateTags_ShouldHaveValidationError()
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Valid Title",
            "<p>Valid content</p>",
            null,
            null,
            null,
            null,
            null,
            new List<string> { "tag1", "tag2", "tag1" } // Duplicate tag1
        );

        // Act
        TestValidationResult<CreateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Tags)
            .WithErrorMessage("Duplicate tags are not allowed");
    }

    [Fact]
    public void Validate_EmptyTags_ShouldHaveValidationError()
    {
        // Arrange
        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            "Valid Title",
            "<p>Valid content</p>",
            null,
            null,
            null,
            null,
            null,
            new List<string> { "tag1", "", "tag2" } // Empty tag
        );

        // Act
        TestValidationResult<CreateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Tags)
            .WithErrorMessage("Tag cannot be empty");
    }

    [Fact]
    public void Validate_MultipleValidationErrors_ShouldHaveAllErrors()
    {
        // Arrange
        string longTitle = new string('A', 201);
        string longSummary = new string('B', 501);
        string longAuthor = new string('C', 101);

        CreateNewsArticleCommand command = new CreateNewsArticleCommand(
            longTitle,
            "", // Empty content
            longSummary,
            new List<string> { "invalid-url" },
            longAuthor,
            "InvalidCategory",
            "InvalidSport",
            new List<string> { "tag1", "tag1" } // Duplicate tags
        );

        // Act
        TestValidationResult<CreateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
        result.ShouldHaveValidationErrorFor(x => x.ContentHtml);
        result.ShouldHaveValidationErrorFor(x => x.Summary);
        result.ShouldHaveValidationErrorFor(x => x.Author);
        result.ShouldHaveValidationErrorFor(x => x.Category);
        result.ShouldHaveValidationErrorFor(x => x.SportCategory);
        result.ShouldHaveValidationErrorFor(x => x.ImageUrls);
        result.ShouldHaveValidationErrorFor(x => x.Tags);
    }
} 