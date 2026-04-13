namespace Cadence.API.Middleware;

public class ApiKeyMiddleware(RequestDelegate next, IConfiguration config)
{
    private static readonly string[] PublicPaths = ["/health", "/openapi"];

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (PublicPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("x-tkey", out var key)
            || key != config["ApiKey"])
        {
            context.Response.StatusCode = 401;
            return;
        }

        await next(context);
    }
}
