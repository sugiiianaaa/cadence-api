namespace Cadence.CLI.Client;

internal sealed record GetTodayHabitsResponse(List<TodayHabitDto> Habits);

internal sealed record TodayHabitDto(
    long Id,
    string Name,
    string Color,
    bool IsCompleted,
    int Streak,
    TimeWindowDto? TimeWindow);

internal sealed record TimeWindowDto(TimeOnly Start, TimeOnly End);

internal sealed record HeatmapDayDto(DateOnly Date, int Completed, int Total);

internal sealed record PutCompletionRequest(bool Completed);

internal sealed record GetHabitOutputDto(
    long Id,
    string Name,
    string Color,
    TimeWindowDto? TimeWindow,
    List<string> ScheduledDays,
    List<DateOnly> RecentCompletions,
    int CurrentStreak);

internal sealed record CompletionResultDto(int CurrentStreak);

internal sealed record CreateHabitRequest(
    string Name,
    string Color,
    TimeWindowDto TimeWindow,
    List<DayOfWeek> ScheduledDays);
