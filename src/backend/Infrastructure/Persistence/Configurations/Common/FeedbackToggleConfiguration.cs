// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Common
{
    /// <summary>
    /// Entity framework configuration for the FeedbackToggle
    /// </summary>
    public class FeedbackToggleConfiguration : IEntityTypeConfiguration<FeedbackToggleEntity>
    {
        /// <summary>
        /// Configures the mapping for the FeedbackToggle entity
        /// </summary>
        /// <param name="builder">The entity type builder</param>
        public void Configure(EntityTypeBuilder<FeedbackToggleEntity> builder)
        {
            //Primary key
            builder.HasKey(f  => f.Id);

            //ID property
            builder.Property(f => f.Id)
                .IsRequired()
                .ValueGeneratedNever();

            //IsEnabled property
            builder.Property(f => f.IsEnabled)
                .IsRequired();

            //CreatedAt property
            builder.Property(f => f.CreatedAt)
                .IsRequired();

            //UpdatedAt property
            builder.Property(f => f.UpdatedAt);
        }
    }
}
