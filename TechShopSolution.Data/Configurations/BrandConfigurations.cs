﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TechShopSolution.Data.Entities;

namespace TechShopSolution.Data.Configurations
{
    public class BrandConfigurations : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("Brand");
            builder.HasKey(x => x.id);
            builder.Property(x => x.brand_name).IsRequired().HasMaxLength(255);
            builder.Property(x => x.brand_slug).IsRequired().HasMaxLength(255).IsUnicode(false);
            builder.Property(x => x.isActive).IsRequired();
            builder.Property(x => x.isDelete).IsRequired();
            builder.Property(x => x.create_at)
              .HasDefaultValueSql("GetDate()");
            builder.Property(x => x.update_at);
            builder.Property(x => x.delete_at);
        }
    }
}
