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
            command.MainImage,
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
