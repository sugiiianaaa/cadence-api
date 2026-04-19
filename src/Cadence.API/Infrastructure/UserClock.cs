namespace Cadence.API.Infrastructure;

public static class UserClock
{
    public const string HeaderName = "X-Timezone";

    public static DateOnly TodayFor(HttpContext ctx)
    {
        if (!ctx.Request.Headers.TryGetValue(HeaderName, out var values) || values.Count == 0)
            throw new BadHttpRequestException(
                $"{HeaderName} header is required (IANA, e.g. 'Asia/Jakarta').");

        var id = values[0]!;
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(id, out var tz))
            throw new BadHttpRequestException($"Unknown timezone '{id}'. Use IANA format.");

        var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        return DateOnly.FromDateTime(localNow);
    }
}
