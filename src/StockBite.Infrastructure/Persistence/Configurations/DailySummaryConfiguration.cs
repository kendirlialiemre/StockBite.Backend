using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockBite.Domain.Entities;

namespace StockBite.Infrastructure.Persistence.Configurations;

public class DailySummaryConfiguration : IEntityTypeConfiguration<DailySummary>
{
    public void Configure(EntityTypeBuilder<DailySummary> builder)
    {
        builder.HasKey(d => d.Id);
        builder.HasIndex(d => new { d.TenantId, d.Date }).IsUnique();
        builder.Property(d => d.TotalRevenue).HasPrecision(12, 2);
        builder.Property(d => d.TotalCost).HasPrecision(12, 2);
        builder.Property(d => d.GrossProfit).HasPrecision(12, 2);
    }
}
