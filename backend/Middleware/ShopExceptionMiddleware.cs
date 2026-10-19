using System.Text.Json;
using ShoppingApi.Services;

namespace ShoppingApi.Middleware;

public class ShopExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ShopExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ShopException exception)
        {
            context.Response.StatusCode = exception.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = exception.Message }));
        }
    }
}
