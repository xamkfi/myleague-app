using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Common
{
    /// <summary>
    /// Entity Framework configuration for the Person entity.
    /// </summary>
    public class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        /// <summary>
        /// Configures the entity mapping for Person.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.BirthDate)
                .IsRequired();

            // Since Person is abstract, we use table-per-hierarchy mapping
            builder.UseTptMappingStrategy();
        }
    }
} 