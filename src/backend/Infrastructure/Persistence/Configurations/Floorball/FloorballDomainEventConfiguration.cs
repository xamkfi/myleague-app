using Domain.DomainEvents;
using Domain.DomainEvents.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    public class FloorballDomainEventConfiguration : IEntityTypeConfiguration<FloorballDomainEvent>
    {
        public void Configure(EntityTypeBuilder<FloorballDomainEvent> builder)
        {
            builder.ToTable("FloorballDomainEvents");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();

            builder.Property(e => e.EventType).HasMaxLength(255).IsRequired();

            builder.Property(e => e.Data)
                .IsRequired()
                .HasColumnType("jsonb"); // Use jsonb for better performance and indexing in PostgreSQL

            builder.Property(e => e.OccurredOn).IsRequired();

            builder.Property(e => e.AggregateId).IsRequired();
            builder.HasIndex(e => e.AggregateId);

            builder.Property<int>("Version").IsRequired();
            builder.HasIndex(new[] { "AggregateId", "Version" }, "IX_FloorballDomainEvents_AggregateId_Version").IsUnique();
        }
    }
} 
