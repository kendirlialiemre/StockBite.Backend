namespace StockBite.Application.Tenants.DTOs;

public record TenantDto(Guid Id, string Name, string Slug, bool IsActive, DateTime CreatedAt);
public record TenantModuleDto(int ModuleId, string ModuleName, bool IsActive, bool GrantedByAdmin, DateTime StartsAt, DateTime? ExpiresAt);
