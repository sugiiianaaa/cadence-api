using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cadence.CLI.Client;

internal sealed class CadenceClient
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;

    private CadenceClient(string baseUrl, string apiKey, string timezone)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + '/') };
        _http.DefaultRequestHeaders.Add("x-tkey", apiKey);
        _http.DefaultRequestHeaders.Add("X-Timezone", timezone);
    }

    public static CadenceClient FromEnv()
    {
        string baseUrl = Environment.GetEnvironmentVariable("CADENCE_API_URL") ?? "http://localhost:5154";
        string apiKey = Environment.GetEnvironmentVariable("CADENCE_API_KEY")
                     ?? throw new InvalidOperationException("CADENCE_API_KEY environment variable is not set.");
        string timezone = Environment.GetEnvironmentVariable("CADENCE_TIMEZONE")
                       ?? TimeZoneInfo.Local.Id;

        return new CadenceClient(baseUrl, apiKey, timezone);
    }

    public async Task<List<TodayHabitDto>> GetTodayAsync(CancellationToken ct = default)
    {
        var res = await _http.GetAsync("api/habits/today", ct);
        await EnsureSuccessAsync(res);

        var output = await res.Content.ReadFromJsonAsync<GetTodayHabitsResponse>(JsonOpts, ct);
        return output?.Habits
               ?? throw new InvalidOperationException("Unexpected null response from api/habits/today");
    }

    public async Task SetCompletionAsync(long habitId, DateOnly date, bool completed, CancellationToken ct = default)
    {
        var res = await _http.PutAsJsonAsync(
            requestUri: $"api/habits/{habitId}/completions/{date:yyyy-MM-dd}",
            value: new PutCompletionRequest(completed),
            options: JsonOpts,
            cancellationToken: ct);
        await EnsureSuccessAsync(res);
    }

    public async Task<List<HeatmapDayDto>> GetHeatmapAsync(int weeks = 4, CancellationToken ct = default)
    {
        var res = await _http.GetAsync($"api/habits/heatmap?weeks={weeks}", ct);
        await EnsureSuccessAsync(res);

        var output = await res.Content.ReadFromJsonAsync<List<HeatmapDayDto>>(JsonOpts, ct);
        return output
               ?? throw new InvalidOperationException("Unexpected null response from api/habits/heatmap");
    }

    public async Task ArchiveHabitAsync(long habitId, CancellationToken ct = default)
    {
        var res = await _http.DeleteAsync($"api/habits/{habitId}", ct);
        await EnsureSuccessAsync(res);
    }

    public async Task<List<GetHabitOutputDto>> GetAllHabitsAsync(CancellationToken ct = default)
    {
        var res = await _http.GetAsync("api/habits", ct);
        await EnsureSuccessAsync(res);

        var output = await res.Content.ReadFromJsonAsync<List<GetHabitOutputDto>>(JsonOpts, ct);
        return output
            ?? throw new InvalidOperationException("Unexpected null response from api/habits");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage res)
    {
        if (!res.IsSuccessStatusCode)
        {
            string body = await res.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"API error {(int)res.StatusCode}: {body}");
        }
    }
}
