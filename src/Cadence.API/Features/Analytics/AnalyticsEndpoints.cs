namespace Cadence.API.Features.Analytics;

// Contract + field-name dictionary lives in docs/analytics.md. Check that
// before renaming or adding fields — mobile parses against the doc.
public static class AnalyticsEndpoints
{
    public static IEndpointRouteBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var analytics = app.MapGroup("/api/analytics").WithTags("Analytics");

        analytics.MapGet("/", GetAnalytics.Handle)
            .WithName(nameof(GetAnalytics))
            .WithSummary("Analytics across all unarchived habits")
            .WithDescription("Returns overall rate, trend, weekday profile, and per-habit metrics for the requested period (week|month|quarter).");

        return app;
    }
}