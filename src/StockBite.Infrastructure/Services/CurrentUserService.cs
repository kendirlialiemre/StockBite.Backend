using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using StockBite.Application.Common.Interfaces;

namespace StockBite.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? User?.FindFirstValue("sub"), out var id) ? id : null;

    public Guid? TenantId =>
        Guid.TryParse(User?.FindFirstValue("tenantId"), out var id) ? id : null;

    // JWT middleware "role" claim'ini ClaimTypes.Role URI'sine map edebilir, her ikisini de dene
    public string? Role =>
        User?.FindFirstValue(ClaimTypes.Role) ?? User?.FindFirstValue("role");

    public IReadOnlyList<string> Permissions =>
        User?.FindAll("permissions").Select(c => c.Value).ToList() ?? [];

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
    public bool IsSuperAdmin => Role == "SuperAdmin";
    public bool IsOwner => Role == "Owner";
}
