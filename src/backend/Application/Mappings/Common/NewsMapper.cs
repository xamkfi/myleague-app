using Application.Commands.News;
using Application.DTOs.Common;
using Domain.Entities.Common;
using Domain.Enums.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mappings.Common;

/// <summary>
/// Mapper class for News entity and related DTOs
/// </summary>
public static class NewsMapper
{
    /// <summary>
    /// Maps a News entity to a NewsDto
    /// </summary>
    /// <param name="news">The News entity to map</param>
    /// <returns>A NewsDto representing the News entity</returns>
    /// <exception cref="ArgumentNullException">Thrown if news is null</exception>
    public static NewsDto ToDto(News news)
    {
        if (news == null)
            throw new ArgumentNullException(nameof(news));

        return new NewsDto(
            news.Id,
            news.Title,
            news.ContentHtml,
            news.Summary,
            news.ImageUrls.Select(url => url.ToString()).ToList().AsReadOnly(),
            news.Author,
            news.CreatedAt,
            news.UpdatedAt,
            news.Category?.ToString(),
            news.SportCategory?.ToString(),
            news.Tags,
            news.IsArchived
        );
    }

    /// <summary>
    /// Maps a News entity to a NewsListDto (without content)
    /// </summary>
    /// <param name="news">The News entity to map</param>
    /// <returns>A NewsListDto representing the News entity</returns>
    /// <exception cref="ArgumentNullException">Thrown if news is null</exception>
    public static NewsListDto ToListDto(News news)
    {
        if (news == null)
            throw new ArgumentNullException(nameof(news));

        return new NewsListDto(
            news.Id,
            news.Title,
            news.Summary,
            news.Author,
            news.CreatedAt,
            news.Category?.ToString(),
            news.SportCategory?.ToString(),
            news.Tags,
            news.IsArchived
        );
    }

    /// <summary>
    /// Maps a collection of News entities to a collection of NewsDtos
    /// </summary>
    /// <param name="newsCollection">The collection of News entities to map</param>
    /// <returns>A collection of NewsDtos</returns>
    /// <exception cref="ArgumentNullException">Thrown if newsCollection is null</exception>
    public static IEnumerable<NewsDto> ToDtos(IEnumerable<News> newsCollection)
    {
        if (newsCollection == null)
            throw new ArgumentNullException(nameof(newsCollection));

        return newsCollection.Select(news => ToDto(news));
    }

    /// <summary>
    /// Maps a collection of News entities to a collection of NewsListDtos
    /// </summary>
    /// <param name="newsCollection">The collection of News entities to map</param>
    /// <returns>A collection of NewsListDtos</returns>
    /// <exception cref="ArgumentNullException">Thrown if newsCollection is null</exception>
    public static IEnumerable<NewsListDto> ToListDtos(IEnumerable<News> newsCollection)
    {
        if (newsCollection == null)
            throw new ArgumentNullException(nameof(newsCollection));

        return newsCollection.Select(news => ToListDto(news));
    }

    /// <summary>
    /// Maps a CreateNewsCommand to a News entity
    /// </summary>
    /// <param name="command">The CreateNewsCommand to map</param>
    /// <returns>A new News entity</returns>
    /// <exception cref="ArgumentNullException">Thrown if command is null</exception>
    public static News ToEntity(CreateNewsCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        Guid newsId = Guid.NewGuid();
        News news = new News(newsId, command.Title, command.ContentHtml, command.Author);

        // Set optional properties if provided
        if (!string.IsNullOrEmpty(command.Summary))
        {
            news.UpdateContent(command.Title, command.ContentHtml, command.Summary);
        }

        if (command.ImageUrls != null && command.ImageUrls.Any())
        {
            foreach (string imageUrl in command.ImageUrls)
            {
                if (Uri.TryCreate(imageUrl, UriKind.Absolute, out Uri? uri))
                {
                    news.SetImage(uri);
                }
            }
        }

        if (!string.IsNullOrEmpty(command.Category) && Enum.TryParse<NewsCategory>(command.Category, true, out NewsCategory category))
        {
            news.SetCategory(category);
        }

        if (!string.IsNullOrEmpty(command.SportCategory) && Enum.TryParse<SportsCategory>(command.SportCategory, true, out SportsCategory sportCategory))
        {
            news.SetSportCategory(sportCategory);
        }

        if (command.Tags != null && command.Tags.Any())
        {
            foreach (string tag in command.Tags)
            {
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    news.AddTag(tag);
                }
            }
        }

        return news;
    }

    /// <summary>
    /// Updates a News entity with values from an UpdateNewsCommand
    /// </summary>
    /// <param name="news">The News entity to update</param>
    /// <param name="command">The UpdateNewsCommand containing updated values</param>
    /// <exception cref="ArgumentNullException">Thrown if news or command is null</exception>
    public static void UpdateFromCommand(News news, UpdateNewsCommand command)
    {
        if (news == null)
            throw new ArgumentNullException(nameof(news));
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Update content
        news.UpdateContent(command.Title, command.ContentHtml, command.Summary);

        // Update category if provided
        if (!string.IsNullOrEmpty(command.Category) && Enum.TryParse<NewsCategory>(command.Category, true, out NewsCategory category))
        {
            news.SetCategory(category);
        }

        // Update sport category if provided
        if (!string.IsNullOrEmpty(command.SportCategory) && Enum.TryParse<SportsCategory>(command.SportCategory, true, out SportsCategory sportCategory))
        {
            news.SetSportCategory(sportCategory);
        }

        // Note: Images and tags are handled separately via dedicated commands
        // to maintain granular control and proper domain event generation
    }

    /// <summary>
    /// Maps available NewsCategory enum values to NewsCategoryDto objects
    /// </summary>
    /// <returns>A collection of NewsCategoryDto objects</returns>
    public static IEnumerable<NewsCategoryDto> GetCategoryDtos()
    {
        return Enum.GetValues<NewsCategory>()
            .Select(category => new NewsCategoryDto(
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