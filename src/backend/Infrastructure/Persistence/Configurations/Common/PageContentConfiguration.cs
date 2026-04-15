// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Common;

public class PageContentConfiguration : IEntityTypeConfiguration<PageContent>
{
    public void Configure(EntityTypeBuilder<PageContent> builder)
    {
        builder.ToTable("PageContents", "common");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PageSlug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(x => x.PageSlug)
            .IsUnique();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ContentHtml)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(256);
    }
}
