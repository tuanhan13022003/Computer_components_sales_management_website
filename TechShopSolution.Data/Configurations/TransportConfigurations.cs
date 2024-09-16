﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TechShopSolution.Data.Entities;

namespace TechShopSolution.Data.Configurations
{
    class TransportConfigurations : IEntityTypeConfiguration<Transport>
    {
        public void Configure(EntityTypeBuilder<Transport> builder)
        {
            builder.ToTable("Transport");
            builder.HasKey(x => x.id);
            builder.Property(x => x.id).UseIdentityColumn(100000,1);
            builder.Property(x => x.cod_price).IsRequired();
            builder.Property(x => x.lading_code).IsUnicode(false);
            builder.Property(x => x.transporter_id).IsRequired();
            builder.Property(x => x.order_id).IsRequired();
            builder.Property(x => x.ship_status).IsRequired();
            builder.Property(x => x.create_at)
                .HasDefaultValueSql("GetDate()");
            builder.Property(x => x.update_at);
            builder.HasAlternateKey(x => x.order_id);
            builder.HasOne(x => x.Transporter).WithMany(t => t.Transports).HasForeignKey(x => x.transporter_id);
        }
    }
}
