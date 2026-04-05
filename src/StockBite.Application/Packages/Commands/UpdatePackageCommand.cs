using MediatR;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Application.Packages.DTOs;
using StockBite.Domain.Entities;
using StockBite.Domain.Enums;

namespace StockBite.Application.Packages.Commands;

public class UpdatePackageCommand : IRequest<PackageDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? DurationDays { get; set; }
    public bool IsActive { get; set; }
    public List<int> ModuleTypes { get; set; } = [];
    public UpdatePackageCommand(Guid id, string name, string description, decimal price, int? durationDays, bool isActive, List<int> moduleTypes)
    { Id = id; Name = name; Description = description; Price = price; DurationDays = durationDays; IsActive = isActive; ModuleTypes = moduleTypes; }
}

public class UpdatePackageCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdatePackageCommand, PackageDto>
{
    public async Task<PackageDto> Handle(UpdatePackageCommand request, CancellationToken ct)
    {
        // Load package WITHOUT navigation property to avoid change tracking conflicts
        var package = await db.Packages
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Package), request.Id);

        package.Name = request.Name;
        package.Description = request.Description;
        package.Price = request.Price;
        package.DurationDays = request.DurationDays;
        package.IsActive = request.IsActive;

        // Load and remove existing modules separately (no Clear() on navigation property)
        var existingModules = await db.PackageModules
            .Where(m => m.PackageId == request.Id)
            .ToListAsync(ct);
        db.PackageModules.RemoveRange(existingModules);

        foreach (var mt in request.ModuleTypes.Distinct())
            db.PackageModules.Add(new PackageModule { PackageId = package.Id, ModuleType = (ModuleType)mt });

        await db.SaveChangesAsync(ct);

        var modules = await db.PackageModules
            .Where(m => m.PackageId == package.Id)
            .ToListAsync(ct);

        return new PackageDto(package.Id, package.Name, package.Description, package.Price, package.DurationDays, package.IsActive,
            modules.Select(m => new PackageModuleDto((int)m.ModuleType, m.ModuleType.ToString())).ToList());
    }
}
