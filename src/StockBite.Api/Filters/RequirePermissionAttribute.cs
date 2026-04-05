using Microsoft.AspNetCore.Mvc.Filters;
using StockBite.Application.Common.Exceptions;
using StockBite.Application.Common.Interfaces;

namespace StockBite.Api.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class RequirePermissionAttribute(string permission) : Attribute, IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
        if (currentUser.IsSuperAdmin || currentUser.IsOwner) return;
        if (!currentUser.Permissions.Contains(permission))
            throw new ForbiddenException($"Bu işlem için '{permission}' yetkisi gerekli.");
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
