using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockBite.Domain.Entities;

namespace StockBite.Infrastructure.Persistence.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Price).HasPrecision(10, 2);
        builder.Property(m => m.CostPrice).HasPrecision(10, 2);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(200);
    }
}
