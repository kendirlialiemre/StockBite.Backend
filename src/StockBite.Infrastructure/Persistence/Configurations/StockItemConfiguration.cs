using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockBite.Domain.Entities;

namespace StockBite.Infrastructure.Persistence.Configurations;

public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Quantity).HasPrecision(10, 3);
        builder.Property(s => s.LowStockThreshold).HasPrecision(10, 3);
        builder.Property(s => s.UnitCost).HasPrecision(10, 2);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Unit).HasMaxLength(50);
    }
}
