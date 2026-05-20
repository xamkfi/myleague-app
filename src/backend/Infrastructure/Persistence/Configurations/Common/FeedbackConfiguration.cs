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
    public class FeedbackConfiguration : IEntityTypeConfiguration<FeedbackEntity>
    {
        public void Configure(EntityTypeBuilder<FeedbackEntity> builder)
        {

            //Primary key
            builder.HasKey(f => f.Id);

            // ID property
            builder.Property(f => f.Id)
                .IsRequired()
                .ValueGeneratedNever();

            //Title property - Max 255 characters
            builder.Property(f => f.Title)
                .IsRequired()
                .HasMaxLength(255);

            //Body property
            builder.Property(f => f.FeedbackBody)
                .IsRequired();

            //CreatedAt property
            builder.Property(f => f.CreatedAt)
                .IsRequired();

            //Email property - Optional, max 255 characters
            builder.Property(f => f.Email)
                .HasMaxLength(255);

            //Performance Indexes

            //Index for getting most recent feedback
            builder.HasIndex(f => f.CreatedAt)
                .HasDatabaseName("IX_Feedback_CreatedAt")
                .IsDescending();
        }
    }
}
