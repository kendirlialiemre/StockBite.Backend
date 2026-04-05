using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockBite.Domain.Entities;

namespace StockBite.Infrastructure.Persistence.Configurations;

public class MenuQrCodeConfiguration : IEntityTypeConfiguration<MenuQrCode>
{
    public void Configure(EntityTypeBuilder<MenuQrCode> builder)
    {
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Label).IsRequired().HasMaxLength(200);
        builder.Property(q => q.FilePath).IsRequired().HasMaxLength(500);
        builder.Property(q => q.PublicUrl).IsRequired().HasMaxLength(1000);
        builder.Property(q => q.PrimaryColor).HasMaxLength(20);
        builder.Property(q => q.BgColor).HasMaxLength(20);
        builder.Property(q => q.TextColor).HasMaxLength(20);
        builder.Property(q => q.FontFamily).HasMaxLength(50);
        builder.Property(q => q.LogoUrl).HasMaxLength(500);
    }
}
