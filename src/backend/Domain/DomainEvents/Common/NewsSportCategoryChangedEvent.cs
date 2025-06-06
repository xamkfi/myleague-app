using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums.Common;

namespace Domain.DomainEvents.Common
{
    public record NewsSportCategoryChangedEvent : IDomainEvent
    {
        /// <summary>
        /// Gets the unique identifier of the event
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the date and time when the event occurred
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Gets the ID of the news article
        /// </summary>
        public Guid NewsId { get; }

        /// <summary>
        /// Gets the previous category
        /// </summary>
        public SportsCategory? OldCategory { get; }

        /// <summary>
        /// Gets the new category
        /// </summary>
        public SportsCategory? NewCategory { get; }

        /// <summary>
        /// Gets the date and time when the category was changed
        /// </summary>
        public DateTime UpdatedAt { get; }

        /// <summary>
        /// Initializes a new instance of the NewsCategoryChangedEvent class
        /// </summary>
        /// <param name="newsId">The ID of the news article</param>
        /// <param name="oldCategory">The previous category</param>
        /// <param name="newCategory">The new category</param>
        /// <param name="updatedAt">The date and time when the category was changed</param>
        public NewsSportCategoryChangedEvent(Guid newsId, SportsCategory? oldCategory, SportsCategory? newCategory, DateTime updatedAt)
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            NewsId = newsId;
            OldCategory = oldCategory;
            NewCategory = newCategory;
            UpdatedAt = updatedAt;
        }
    }
}
