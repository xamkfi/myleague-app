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

            builder.Property(x => x.IsRegistered)
                .HasDefaultValue(false)
                .IsRequired();

            // Configure owned types for value objects
            builder.OwnsOne(p => p.Address, addressBuilder =>
            {
                addressBuilder.Property(a => a.Street1).HasMaxLength(200);
                addressBuilder.Property(a => a.Street2).HasMaxLength(200);
                addressBuilder.Property(a => a.City).HasMaxLength(100);
                addressBuilder.Property(a => a.PostalCode).HasMaxLength(20);
                addressBuilder.Property(a => a.Country).HasMaxLength(100);
            });

            builder.OwnsOne(p => p.ContactInfo, contactBuilder =>
            {
                contactBuilder.Property(c => c.Email).HasMaxLength(255);
                contactBuilder.Property(c => c.Phone).HasMaxLength(50);
                contactBuilder.Property(c => c.AlternativePhone).HasMaxLength(50);
            });
        }
    }
} 
