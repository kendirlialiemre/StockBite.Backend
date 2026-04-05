using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockBite.Domain.Entities;

namespace StockBite.Infrastructure.Persistence.Configurations;

public class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Price).HasPrecision(10, 2);
        builder.HasMany(p => p.Modules).WithOne(m => m.Package).HasForeignKey(m => m.PackageId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PackageModuleConfiguration : IEntityTypeConfiguration<PackageModule>
{
    public void Configure(EntityTypeBuilder<PackageModule> builder)
    {
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => new { m.PackageId, m.ModuleType }).IsUnique();
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<StockBite.Domain.Entities.Payment>
{
    public void Configure(EntityTypeBuilder<StockBite.Domain.Entities.Payment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Amount).HasPrecision(10, 2);
        builder.HasOne(p => p.Package).WithMany().HasForeignKey(p => p.PackageId).OnDelete(DeleteBehavior.Restrict);
    }
}
