namespace StockBite.Application.Packages.DTOs;
public record PackageModuleDto(int ModuleType, string ModuleName);
public record PackageDto(Guid Id, string Name, string Description, decimal Price, int? DurationDays, bool IsActive, IReadOnlyList<PackageModuleDto> Modules);
