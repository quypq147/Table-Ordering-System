using Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, logger);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext ctx, Exception ex, ILogger logger)
    {
        var traceId = ctx.TraceIdentifier;
        int status = (int)HttpStatusCode.InternalServerError;
        string code = "error.internal";

        switch (ex)
        {
            case DomainException de:
                status = de.HttpStatusOverride ?? (int)HttpStatusCode.BadRequest;
                code = de.Code;
                break;
            case KeyNotFoundException:
                status = (int)HttpStatusCode.NotFound;
                code = "error.not_found";
                break;
            case UnauthorizedAccessException:
                status = (int)HttpStatusCode.Unauthorized;
                code = "error.unauthorized";
                break;
            case InvalidOperationException:
                status = (int)HttpStatusCode.BadRequest;
                code = "error.invalid_operation";
                break;
        }

        logger.LogError(ex, "Unhandled exception {Code} {TraceId}", code, traceId);
        var payload = new
        {
            traceId,
            code,
            message = ex.Message,
            details = ex.GetType().Name
        };
        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = status;
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
