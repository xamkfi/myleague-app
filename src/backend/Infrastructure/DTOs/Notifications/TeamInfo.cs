using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// Basic team information
    /// </summary>
    public record TeamInfo
    {
        /// <summary>
        /// Gets the ID of the team
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Gets the name of the team
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Default constructor
        /// </summary>
        public TeamInfo()
        {
            Id = Guid.Empty;
        }
    }
} 