using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;
using StockBite.Domain.Enums;

namespace StockBite.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireModuleAttribute(ModuleType module) : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
        if (currentUser.IsSuperAdmin) { await next(); return; }

        var tenantId = currentUser.TenantId;
        if (tenantId == null) throw new ForbiddenException();

        var db = context.HttpContext.RequestServices.GetRequiredService<IApplicationDbContext>();
        var hasModule = await db.TenantModules.IgnoreQueryFilters()
            .AnyAsync(tm => tm.TenantId == tenantId
                         && tm.ModuleType == module
                         && tm.IsActive
                         && (tm.ExpiresAt == null || tm.ExpiresAt > DateTime.UtcNow));

        if (!hasModule) throw new ModuleNotSubscribedException(module);
        await next();
    }
}
