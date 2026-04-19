using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Cadence.API.Infrastructure;

public class BadRequestExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext ctx, Exception ex, CancellationToken ct)
    {
        if (ex is not BadHttpRequestException bad) return false;

        var status = bad.StatusCode == 0 ? StatusCodes.Status400BadRequest : bad.StatusCode;
        ctx.Response.StatusCode = status;

        await ctx.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = "Bad Request",
            Detail = bad.Message,
        }, ct);

        return true;
    }
}
