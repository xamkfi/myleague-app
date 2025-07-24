using Application.Commands.NewsArticles;
using Application.DTOs.Common;
using Domain.Entities.Common;
using Domain.Enums.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mappings.Common;

/// <summary>
/// Mapper class for NewsArticle entity and related DTOs
/// </summary>
public static class NewsArticleMapper
{
            /// <summary>
        /// Maps a NewsArticle entity to a NewsArticleDto
        /// </summary>
        /// <param name="newsArticle">The NewsArticle entity to map</param>
        /// <returns>A NewsArticleDto representing the NewsArticle entity</returns>
        /// <exception cref="ArgumentNullException">Thrown if newsArticle is null</exception>
        public static NewsArticleDto ToDto(NewsArticle newsArticle)
    {
        if (newsArticle == null)
            throw new ArgumentNullException(nameof(newsArticle));

        return new NewsArticleDto(
            newsArticle.Id,
            newsArticle.Title,
            newsArticle.MainImage,
            newsArticle.ContentHtml,
            newsArticle.Summary,
            newsArticle.ImageUrls.Select(url => url.ToString()).ToList().AsReadOnly(),
            newsArticle.Author,
            newsArticle.CreatedAt,
            newsArticle.UpdatedAt,
            newsArticle.Category?.ToString(),
            newsArticle.SportCategory?.ToString(),
            newsArticle.Tags,
            newsArticle.IsArchived
        );
    }

            /// <summary>
        /// Maps a NewsArticle entity to a NewsArticleListDto (without content)
        /// </summary>
        /// <param name="newsArticle">The NewsArticle entity to map</param>
        /// <returns>A NewsArticleListDto representing the NewsArticle entity</returns>
        /// <exception cref="ArgumentNullException">Thrown if newsArticle is null</exception>
        public static NewsArticleListDto ToListDto(NewsArticle newsArticle)
    {
        if (newsArticle == null)
            throw new ArgumentNullException(nameof(newsArticle));

        return new NewsArticleListDto(
            newsArticle.Id,
            newsArticle.Title,
            newsArticle.MainImage,
            newsArticle.Summary,
            newsArticle.Author,
            newsArticle.CreatedAt,
            newsArticle.Category?.ToString(),
            newsArticle.SportCategory?.ToString(),
            newsArticle.Tags,
            newsArticle.IsArchived
        );
    }

            /// <summary>
        /// Maps a collection of NewsArticle entities to a collection of NewsArticleDtos
        /// </summary>
        /// <param name="newsArticleCollection">The collection of NewsArticle entities to map</param>
        /// <returns>A collection of NewsArticleDtos</returns>
        /// <exception cref="ArgumentNullException">Thrown if newsArticleCollection is null</exception>
        public static IEnumerable<NewsArticleDto> ToDtos(IEnumerable<NewsArticle> newsArticleCollection)
    {
        if (newsArticleCollection == null)
            throw new ArgumentNullException(nameof(newsArticleCollection));

        return newsArticleCollection.Select(newsArticle => ToDto(newsArticle));
    }

            /// <summary>
        /// Maps a collection of NewsArticle entities to a collection of NewsArticleListDtos
        /// </summary>
        /// <param name="newsArticleCollection">The collection of NewsArticle entities to map</param>
        /// <returns>A collection of NewsArticleListDtos</returns>
        /// <exception cref="ArgumentNullException">Thrown if newsArticleCollection is null</exception>
        public static IEnumerable<NewsArticleListDto> ToListDtos(IEnumerable<NewsArticle> newsArticleCollection)
    {
        if (newsArticleCollection == null)
            throw new ArgumentNullException(nameof(newsArticleCollection));

        return newsArticleCollection.Select(newsArticle => ToListDto(newsArticle));
    }

            /// <summary>
        /// Maps a CreateNewsArticleCommand to a NewsArticle entity
        /// </summary>
        /// <param name="command">The CreateNewsArticleCommand to map</param>
        /// <returns>A new NewsArticle entity</returns>
        /// <exception cref="ArgumentNullException">Thrown if command is null</exception>
        public static NewsArticle ToEntity(CreateNewsArticleCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        Guid newsId = Guid.NewGuid();
        NewsArticle newsArticle = new NewsArticle(newsId,
            command.Title,
            command.MainImage ?? new Uri("data:image/jpeg;base64,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"),
            command.ContentHtml,
            command.Author);

        // Set optional properties if provided
        if (!string.IsNullOrEmpty(command.Summary))
        {
            newsArticle.UpdateContent(command.Title, command.ContentHtml, command.Summary);
        }

        if (command.ImageUrls != null && command.ImageUrls.Any())
        {
            foreach (string imageUrl in command.ImageUrls)
            {
                if (Uri.TryCreate(imageUrl, UriKind.Absolute, out Uri? uri))
                {
                    newsArticle.SetImage(uri);
                }
            }
        }

        if (!string.IsNullOrEmpty(command.Category) && Enum.TryParse<NewsCategory>(command.Category, true, out NewsCategory category))
        {
            newsArticle.SetCategory(category);
        }

        if (!string.IsNullOrEmpty(command.SportCategory) && Enum.TryParse<SportsCategory>(command.SportCategory, true, out SportsCategory sportCategory))
        {
            newsArticle.SetSportCategory(sportCategory);
        }

        if (command.Tags != null && command.Tags.Any())
        {
            foreach (string tag in command.Tags)
            {
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    newsArticle.AddTag(tag);
                }
            }
        }

        return newsArticle;
    }

            /// <summary>
        /// Updates a NewsArticle entity with values from an UpdateNewsArticleCommand
        /// </summary>
        /// <param name="newsArticle">The NewsArticle entity to update</param>
        /// <param name="command">The UpdateNewsArticleCommand containing updated values</param>
        /// <exception cref="ArgumentNullException">Thrown if newsArticle or command is null</exception>
        public static void UpdateFromCommand(NewsArticle newsArticle, UpdateNewsArticleCommand command)
    {
        if (newsArticle == null)
            throw new ArgumentNullException(nameof(newsArticle));
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Update content
        newsArticle.UpdateContent(command.Title, command.ContentHtml, command.Summary);

        // Update main image if provided
        if (command.MainImage != null)
        {
            newsArticle.SetMainImage(command.MainImage);
        }

        // Update author if provided
        if (command.Author != null)
        {
            newsArticle.UpdateAuthor(command.Author);
        }

        // Update category if provided
        if (!string.IsNullOrEmpty(command.Category) && Enum.TryParse<NewsCategory>(command.Category, true, out NewsCategory category))
        {
            newsArticle.SetCategory(category);
        }

        // Update sport category if provided
        if (!string.IsNullOrEmpty(command.SportCategory) && Enum.TryParse<SportsCategory>(command.SportCategory, true, out SportsCategory sportCategory))
        {
            newsArticle.SetSportCategory(sportCategory);
        }

        // Note: Images and tags are handled separately via dedicated commands
        // to maintain granular control and proper domain event generation
    }

            /// <summary>
        /// Maps available NewsCategory enum values to NewsArticleCategoryDto objects
        /// </summary>
        /// <returns>A collection of NewsArticleCategoryDto objects</returns>
        public static IEnumerable<NewsArticleCategoryDto> GetCategoryDtos()
    {
        return Enum.GetValues<NewsCategory>()
            .Select(category => new NewsArticleCategoryDto(
                category.ToString(),
                GetCategoryDisplayName(category),
                GetCategoryDescription(category)
            ));
    }

    /// <summary>
    /// Gets a display name for a NewsCategory
    /// </summary>
    /// <param name="category">The NewsCategory</param>
    /// <returns>A user-friendly display name</returns>
    private static string GetCategoryDisplayName(NewsCategory category)
    {
        return category switch
        {
            NewsCategory.None => "None",
            NewsCategory.General => "General News",
            NewsCategory.MatchReports => "Match Reports",
            NewsCategory.LeagueNews => "League News",
            NewsCategory.PlayerUpdates => "Player Updates",
            NewsCategory.TeamNews => "Team News",
            NewsCategory.Announcements => "Announcements",
            NewsCategory.Events => "Events",
            NewsCategory.Transfers => "Transfers",
            NewsCategory.Injuries => "Injuries",
            NewsCategory.Awards => "Awards",
            _ => category.ToString()
        };
    }

    /// <summary>
    /// Gets a description for a NewsCategory
    /// </summary>
    /// <param name="category">The NewsCategory</param>
    /// <returns>A description of the category</returns>
    private static string GetCategoryDescription(NewsCategory category)
    {
        return category switch
        {
            NewsCategory.None => "No specific category",
            NewsCategory.General => "General news and updates",
            NewsCategory.MatchReports => "Reports and summaries of matches",
            NewsCategory.LeagueNews => "League-wide news and announcements",
            NewsCategory.PlayerUpdates => "News about individual players",
            NewsCategory.TeamNews => "Team-specific news and updates",
            NewsCategory.Announcements => "Official announcements",
            NewsCategory.Events => "Upcoming events and activities",
            NewsCategory.Transfers => "Player transfers and signings",
            NewsCategory.Injuries => "Injury reports and updates",
            NewsCategory.Awards => "Awards and recognitions",
            _ => "Category description not available"
        };
    }
} 
