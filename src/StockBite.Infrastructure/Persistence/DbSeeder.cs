using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Entities;
using StockBite.Domain.Enums;

namespace StockBite.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, IPasswordHasher passwordHasher)
    {
        await db.Database.MigrateAsync();

        if (!await db.Users.AnyAsync(u => u.Role == UserRole.SuperAdmin))
        {
            var admin = new User
            {
                Email = "admin@stockbite.io",
                PasswordHash = passwordHasher.Hash("Admin123!"),
                FirstName = "Super",
                LastName = "Admin",
                Role = UserRole.SuperAdmin,
                TenantId = null
            };
            db.Users.Add(admin);
            await db.SaveChangesAsync();
            Console.WriteLine("✓ SuperAdmin kullanıcısı oluşturuldu: admin@stockbite.io / Admin123!");
        }
    }
}
