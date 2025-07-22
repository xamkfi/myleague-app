using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Common
{
    /// <summary>
    /// Entity Framework configuration for the Person entity.
    /// </summary>
    public class PersonConfiguration : BaseEntityConfiguration<Person>
    {
        /// <summary>
        /// Configures Person-specific properties.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        protected override void ConfigureEntity(EntityTypeBuilder<Person> builder)
        {
            builder.Property(p => p.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.BirthDate)
                .IsRequired();

            builder.Property(p => p.role)
                .IsRequired()
                .HasDefaultValue(Domain.Enums.Common.PersonRole.User)
                .HasConversion<int>();

            builder.Property(x => x.IsRegistered)
                .HasDefaultValue(false)
                .IsRequired();

            // Configure owned types for value objects
            builder.OwnsOne(p => p.Address, addressBuilder =>
            {
                addressBuilder.Property(a => a.Street1)
                    .IsRequired(false)
                    .HasMaxLength(200);
                addressBuilder.Property(a => a.Street2)
                    .IsRequired(false)
                    .HasMaxLength(200);
                addressBuilder.Property(a => a.City)
                    .IsRequired(false)
                    .HasMaxLength(100);
                addressBuilder.Property(a => a.PostalCode)
                    .IsRequired(false)
                    .HasMaxLength(20);
                addressBuilder.Property(a => a.Country)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            builder.OwnsOne(p => p.ContactInfo, contactBuilder =>
            {
                contactBuilder.Property(c => c.Email)
                    .IsRequired()
                    .HasMaxLength(255);
                contactBuilder.Property(c => c.Phone)
                    .IsRequired(false)
                    .HasMaxLength(50);
                contactBuilder.Property(c => c.AlternativePhone)
                    .IsRequired(false)
                    .HasMaxLength(50);
            });
        }
    }
} 
