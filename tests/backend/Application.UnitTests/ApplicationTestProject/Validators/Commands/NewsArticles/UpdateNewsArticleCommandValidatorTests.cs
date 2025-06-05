using Application.Commands.NewsArticles;
using Application.Validators.Commands.NewsArticles;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using Xunit;

namespace ApplicationTestProject.Validators.Commands.NewsArticles;

public class UpdateNewsArticleCommandValidatorTests
{
    private readonly UpdateNewsArticleCommandValidator _validator;

    public UpdateNewsArticleCommandValidatorTests()
    {
        _validator = new UpdateNewsArticleCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        UpdateNewsArticleCommand command = new UpdateNewsArticleCommand(
            Guid.NewGuid(),
            "Valid Updated Title",
            "<p>Valid updated content</p>",
            "Valid updated summary",
            new List<string> { "https://example.com/updated-image.jpg" },
            "Updated Author",
            "MatchReports",
            "Football",
            new List<string> { "updated-tag1", "updated-tag2" }
        );

        // Act
        TestValidationResult<UpdateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidCommandWithMinimalData_ShouldNotHaveValidationErrors()
    {
        // Arrange
        UpdateNewsArticleCommand command = new UpdateNewsArticleCommand(
            Guid.NewGuid(),
            "Valid Title",
            "<p>Valid content</p>"
        );

        // Act
        TestValidationResult<UpdateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_ShouldHaveValidationError()
    {
        // Arrange
        UpdateNewsArticleCommand command = new UpdateNewsArticleCommand(
            Guid.Empty,
            "Valid Title",
            "<p>Valid content</p>"
        );

        // Act
        TestValidationResult<UpdateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("News article ID is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyOrNullTitle_ShouldHaveValidationError(string? title)
    {
        // Arrange
        UpdateNewsArticleCommand command = new UpdateNewsArticleCommand(
            Guid.NewGuid(),
            title!,
            "<p>Valid content</p>"
        );

        // Act
        TestValidationResult<UpdateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public void Validate_TitleTooLong_ShouldHaveValidationError()
    {
        // Arrange
        string longTitle = new string('A', 201); // 201 characters
        UpdateNewsArticleCommand command = new UpdateNewsArticleCommand(
            Guid.NewGuid(),
            longTitle,
            "<p>Valid content</p>"
        );

        // Act
        TestValidationResult<UpdateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title cannot exceed 200 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyOrNullContent_ShouldHaveValidationError(string? content)
    {
        // Arrange
        UpdateNewsArticleCommand command = new UpdateNewsArticleCommand(
            Guid.NewGuid(),
            "Valid Title",
            content!
        );

        // Act
        TestValidationResult<UpdateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ContentHtml)
            .WithErrorMessage("Content is required");
    }

    [Fact]
    public void Validate_SummaryTooLong_ShouldHaveValidationError()
    {
        // Arrange
        string longSummary = new string('B', 501); // 501 characters
        UpdateNewsArticleCommand command = new UpdateNewsArticleCommand(
            Guid.NewGuid(),
            "Valid Title",
            "<p>Valid content</p>",
            longSummary
        );

        // Act
        TestValidationResult<UpdateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Summary)
            .WithErrorMessage("Summary cannot exceed 500 characters");
    }

    [Fact]
    public void Validate_AuthorTooLong_ShouldHaveValidationError()
    {
        // Arrange
        string longAuthor = new string('C', 101); // 101 characters
        UpdateNewsArticleCommand command = new UpdateNewsArticleCommand(
            Guid.NewGuid(),
            "Valid Title",
            "<p>Valid content</p>",
            null,
            null,
            longAuthor
        );

        // Act
        TestValidationResult<UpdateNewsArticleCommand> result = _validator.TestValidate(command);

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
        UpdateNewsArticleCommand command = new UpdateNewsArticleCommand(
            Guid.NewGuid(),
            "Valid Title",
            "<p>Valid content</p>",
            null,
            null,
            null,
            category
        );

        // Act
        TestValidationResult<UpdateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Category);
    }

    [Theory]
    [InlineData("InvalidCategory")]
    [InlineData("Random")]
    public void Validate_InvalidCategory_ShouldHaveValidationError(string invalidCategory)
    {
        // Arrange
        UpdateNewsArticleCommand command = new UpdateNewsArticleCommand(
            Guid.NewGuid(),
            "Valid Title",
            "<p>Valid content</p>",
            null,
            null,
            null,
            invalidCategory
        );

        // Act
        TestValidationResult<UpdateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage("Invalid news category");
    }

    [Theory]
    [InlineData("https://example.com/image.jpg")]
    [InlineData("http://example.com/image.png")]
    public void Validate_ValidImageUrls_ShouldNotHaveValidationError(string imageUrl)
    {
        // Arrange
        UpdateNewsArticleCommand command = new UpdateNewsArticleCommand(
            Guid.NewGuid(),
            "Valid Title",
            "<p>Valid content</p>",
            null,
            new List<string> { imageUrl }
        );

        // Act
        TestValidationResult<UpdateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ImageUrls);
    }

    [Theory]
    [InlineData("invalid-url")]
    [InlineData("ftp://example.com/image.jpg")]
    public void Validate_InvalidImageUrls_ShouldHaveValidationError(string invalidUrl)
    {
        // Arrange
        UpdateNewsArticleCommand command = new UpdateNewsArticleCommand(
            Guid.NewGuid(),
            "Valid Title",
            "<p>Valid content</p>",
            null,
            new List<string> { invalidUrl }
        );

        // Act
        TestValidationResult<UpdateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ImageUrls)
            .WithErrorMessage("Invalid image URL format");
    }

    [Fact]
    public void Validate_DuplicateTags_ShouldHaveValidationError()
    {
        // Arrange
        UpdateNewsArticleCommand command = new UpdateNewsArticleCommand(
            Guid.NewGuid(),
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
        TestValidationResult<UpdateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Tags)
            .WithErrorMessage("Duplicate tags are not allowed");
    }

    [Fact]
    public void Validate_EmptyTags_ShouldHaveValidationError()
    {
        // Arrange
        UpdateNewsArticleCommand command = new UpdateNewsArticleCommand(
            Guid.NewGuid(),
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
        TestValidationResult<UpdateNewsArticleCommand> result = _validator.TestValidate(command);

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

        UpdateNewsArticleCommand command = new UpdateNewsArticleCommand(
            Guid.Empty, // Invalid ID
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
        TestValidationResult<UpdateNewsArticleCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
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