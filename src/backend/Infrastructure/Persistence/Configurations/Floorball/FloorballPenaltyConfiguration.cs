using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball;

/// <summary>
/// Entity Framework configuration for the FloorballPenalty entity.
/// </summary>
public class FloorballPenaltyConfiguration : IEntityTypeConfiguration<FloorballPenalty>
{
    /// <summary>
    /// Configures the entity mapping for FloorballPenalty.
    /// </summary>
    /// <param name="b"></param>
    public void Configure(EntityTypeBuilder<FloorballPenalty> b)
    {

        b.Property(p => p.PlayerId).IsRequired(false);
        b.Property(p => p.PenaltyType);
        b.Property(p => p.DurationInMinutes);


        b.HasIndex(p => p.PlayerId)
            .HasDatabaseName("IX_FloorballMatchEvent_PlayerId");

        b.HasIndex(p => p.PenaltyType)
            .HasDatabaseName("IX_FloorballMatchEvent_PenaltyType");

        b.HasIndex(p => p.DurationInMinutes)
            .HasDatabaseName("IX_FloorballMatchEvent_DurationInMinutes");
    }
}

