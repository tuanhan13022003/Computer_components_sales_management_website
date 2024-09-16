﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TechShopSolution.Data.Entities;

namespace TechShopSolution.Data.Configurations
{
    public class CategoryNewsConfigurations : IEntityTypeConfiguration<CategoryNews>
    {
        public void Configure(EntityTypeBuilder<CategoryNews> builder)
        {
            builder.ToTable("CategoryNews");
            builder.HasKey(x => x.id);
            builder.Property(x => x.slug).IsRequired().HasMaxLength(255).IsUnicode(false);
            builder.Property(x => x.name).IsRequired().HasMaxLength(255);
            builder.Property(x => x.isActive).IsRequired();
            builder.Property(x => x.isDelete).IsRequired();
            builder.Property(x => x.create_at)
                    .HasDefaultValueSql("GetDate()");
            builder.Property(x => x.update_at);
            builder.Property(x => x.delete_at);
        }
    }
}
