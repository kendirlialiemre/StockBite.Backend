using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockBite.Domain.Entities;

namespace StockBite.Infrastructure.Persistence.Configurations;

public class MenuItemIngredientConfiguration : IEntityTypeConfiguration<MenuItemIngredient>
{
    public void Configure(EntityTypeBuilder<MenuItemIngredient> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Quantity).HasPrecision(10, 3);

        builder.HasOne(i => i.MenuItem)
            .WithMany(m => m.Ingredients)
            .HasForeignKey(i => i.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.StockItem)
            .WithMany()
            .HasForeignKey(i => i.StockItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
