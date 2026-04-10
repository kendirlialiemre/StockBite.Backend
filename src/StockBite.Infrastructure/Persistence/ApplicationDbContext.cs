using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;

namespace StockBite.Infrastructure.Persistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentUserService currentUser) : DbContext(options), IApplicationDbContext
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<TenantModule> TenantModules => Set<TenantModule>();
    public DbSet<TenantUserPermission> TenantUserPermissions => Set<TenantUserPermission>();
    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<MenuItemIngredient> MenuItemIngredients => Set<MenuItemIngredient>();
    public DbSet<Table> Tables => Set<Table>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<StockCategory> StockCategories => Set<StockCategory>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<DailySummary> DailySummaries => Set<DailySummary>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<PackageModule> PackageModules => Set<PackageModule>();
    public DbSet<StockBite.Domain.Entities.Payment> Payments => Set<StockBite.Domain.Entities.Payment>();
    public DbSet<MenuQrCode> MenuQrCodes => Set<MenuQrCode>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<MemberSubscription> MemberSubscriptions => Set<MemberSubscription>();
    public DbSet<SubscriptionSession> SubscriptionSessions => Set<SubscriptionSession>();
    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        ApplyTenantQueryFilters(modelBuilder);
    }

    private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuCategory>().HasQueryFilter(e => currentUser.IsSuperAdmin || e.TenantId == currentUser.TenantId);
        modelBuilder.Entity<MenuItem>().HasQueryFilter(e => currentUser.IsSuperAdmin || e.TenantId == currentUser.TenantId);
        modelBuilder.Entity<Table>().HasQueryFilter(e => currentUser.IsSuperAdmin || e.TenantId == currentUser.TenantId);
        modelBuilder.Entity<Order>().HasQueryFilter(e => currentUser.IsSuperAdmin || e.TenantId == currentUser.TenantId);
        modelBuilder.Entity<OrderItem>().HasQueryFilter(e => currentUser.IsSuperAdmin || e.TenantId == currentUser.TenantId);
        modelBuilder.Entity<StockCategory>().HasQueryFilter(e => currentUser.IsSuperAdmin || e.TenantId == currentUser.TenantId);
        modelBuilder.Entity<StockItem>().HasQueryFilter(e => currentUser.IsSuperAdmin || e.TenantId == currentUser.TenantId);
        modelBuilder.Entity<StockMovement>().HasQueryFilter(e => currentUser.IsSuperAdmin || e.TenantId == currentUser.TenantId);
        modelBuilder.Entity<TenantUserPermission>().HasQueryFilter(e => currentUser.IsSuperAdmin || e.TenantId == currentUser.TenantId);
        modelBuilder.Entity<MenuQrCode>().HasQueryFilter(e => currentUser.IsSuperAdmin || e.TenantId == currentUser.TenantId);
        modelBuilder.Entity<Expense>().HasQueryFilter(e => currentUser.IsSuperAdmin || e.TenantId == currentUser.TenantId);
        modelBuilder.Entity<Member>().HasQueryFilter(e => currentUser.IsSuperAdmin || e.TenantId == currentUser.TenantId);
        modelBuilder.Entity<MemberSubscription>().HasQueryFilter(e => currentUser.IsSuperAdmin || e.TenantId == currentUser.TenantId);
        modelBuilder.Entity<SubscriptionSession>().HasQueryFilter(e => currentUser.IsSuperAdmin || e.TenantId == currentUser.TenantId);
        modelBuilder.Entity<Event>().HasQueryFilter(e => currentUser.IsSuperAdmin || e.TenantId == currentUser.TenantId);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
