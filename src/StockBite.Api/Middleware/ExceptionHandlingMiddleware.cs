using System.Net;
using System.Text.Json;
using StockBite.Application.Common.Exceptions;

namespace StockBite.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "İşlenmeyen hata");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, 
            message, errors) = exception switch
        {
            ValidationException vex => (HttpStatusCode.BadRequest, vex.Message, (object)vex.Errors),
            NotFoundException nex => (HttpStatusCode.NotFound, nex.Message, (object)new { }),
            ForbiddenException fex => (HttpStatusCode.Forbidden, fex.Message, (object)new { }),
            ModuleNotSubscribedException mex => (HttpStatusCode.PaymentRequired, mex.Message, (object)new { }),
            InvalidOperationException iex => (HttpStatusCode.Conflict, iex.Message, (object)new { }),
            _ => (HttpStatusCode.InternalServerError, exception.Message, (object)new { type = exception.GetType().Name, stack = exception.StackTrace })
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new { message, errors };
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
