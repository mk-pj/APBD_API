using System.Net;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace Tutorial8.Middlewares;

public class GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Database error: {Message}", ex.Message);
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "Database error occurred.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(new { error = message });
        await context.Response.WriteAsync(result);
    }
}


public static class GlobalExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandler>();
    }
}
