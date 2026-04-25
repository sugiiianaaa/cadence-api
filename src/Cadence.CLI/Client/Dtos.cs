namespace Cadence.CLI.Client;

record GetTodayHabitsResponse(List<TodayHabitDto> Habits);

record TodayHabitDto(
    long Id,
    string Name,
    string Color,
    bool IsCompleted,
    int Streak,
    TimeWindowDto? TimeWindow);

record TimeWindowDto(TimeOnly Start, TimeOnly End);

record HeatmapDayDto(DateOnly Date, int Completed, int Total);

record PutCompletionRequest(bool Completed);

record GetHabitOutputDto(
    long Id,
    string Name,
    string Color,
    TimeWindowDto? TimeWindow,
    List<string> ScheduledDays,
    List<DateOnly> RecentCompletions);

record CreateHabitRequest(
    string Name,
    string? Description,
    string Color,
    TimeWindowDto TimeWindow,
    List<DayOfWeek> ScheduledDays);
