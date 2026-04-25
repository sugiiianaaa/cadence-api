using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cadence.CLI.Client;

sealed class CadenceClient
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    public CadenceClient(string baseUrl, string apiKey, string timezone)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _http.DefaultRequestHeaders.Add("x-tkey", apiKey);
        _http.DefaultRequestHeaders.Add("X-Timezone", timezone);
    }

    public static CadenceClient FromEnv()
    {
        var baseUrl = Environment.GetEnvironmentVariable("CADENCE_API_URL") ?? "http://localhost:5154";
        var apiKey = Environment.GetEnvironmentVariable("CADENCE_API_KEY")
            ?? throw new InvalidOperationException("CADENCE_API_KEY environment variable is not set.");
        var timezone = Environment.GetEnvironmentVariable("CADENCE_TIMEZONE")
            ?? TimeZoneInfo.Local.Id;
        return new CadenceClient(baseUrl, apiKey, timezone);
    }

    public async Task<List<TodayHabitDto>> GetTodayAsync()
    {
        var response = await _http.GetFromJsonAsync<GetTodayHabitsResponse>("/api/habits/today", JsonOpts);
        return response!.Habits;
    }

    public async Task SetCompletionAsync(long habitId, DateOnly date, bool completed)
    {
        var res = await _http.PutAsJsonAsync(
            $"/api/habits/{habitId}/completions/{date:yyyy-MM-dd}",
            new PutCompletionRequest(completed),
            JsonOpts);
        res.EnsureSuccessStatusCode();
    }

    public async Task<List<HeatmapDayDto>> GetHeatmapAsync(int weeks = 4)
    {
        var result = await _http.GetFromJsonAsync<List<HeatmapDayDto>>(
            $"/api/habits/heatmap?weeks={weeks}", JsonOpts);
        return result!;
    }

    public async Task ArchiveHabitAsync(long habitId)
    {
        var res = await _http.DeleteAsync($"/api/habits/{habitId}");
        res.EnsureSuccessStatusCode();
    }

    public async Task<List<GetHabitOutputDto>> GetAllHabitsAsync()
    {
        var result = await _http.GetFromJsonAsync<List<GetHabitOutputDto>>("/api/habits", JsonOpts);
        return result!;
    }
}
