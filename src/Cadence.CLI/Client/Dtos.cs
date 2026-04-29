namespace Cadence.CLI.Client;

internal record GetTodayHabitsResponse(List<TodayHabitDto> Habits);

internal record TodayHabitDto(
    long Id,
    string Name,
    string Color,
    bool IsCompleted,
    int Streak,
    TimeWindowDto? TimeWindow);

internal record TimeWindowDto(TimeOnly Start, TimeOnly End);

internal record HeatmapDayDto(DateOnly Date, int Completed, int Total);

internal record PutCompletionRequest(bool Completed);

internal record GetHabitOutputDto(
    long Id,
    string Name,
    string Color,
    TimeWindowDto? TimeWindow,
    List<string> ScheduledDays,
    List<DateOnly> RecentCompletions);

internal record CreateHabitRequest(
    string Name,
    string? Description,
    string Color,
    TimeWindowDto TimeWindow,
    List<DayOfWeek> ScheduledDays);
