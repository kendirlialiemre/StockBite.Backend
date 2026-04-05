using Microsoft.EntityFrameworkCore;
using StockBite.Domain.Entities;

namespace StockBite.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<TenantModule> TenantModules { get; }
    DbSet<TenantUserPermission> TenantUserPermissions { get; }
    DbSet<MenuCategory> MenuCategories { get; }
    DbSet<MenuItem> MenuItems { get; }
    DbSet<MenuItemIngredient> MenuItemIngredients { get; }
    DbSet<Table> Tables { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<StockCategory> StockCategories { get; }
    DbSet<StockItem> StockItems { get; }
    DbSet<StockMovement> StockMovements { get; }
    DbSet<DailySummary> DailySummaries { get; }
    DbSet<Package> Packages { get; }
    DbSet<PackageModule> PackageModules { get; }
    DbSet<Payment> Payments { get; }
    DbSet<MenuQrCode> MenuQrCodes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
