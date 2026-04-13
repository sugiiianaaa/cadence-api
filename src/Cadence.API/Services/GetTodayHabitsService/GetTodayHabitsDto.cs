using Cadence.API.Data.ValueObjects;

public record GetTodayHabitsDto(
    List<TodayHabitDto> Habits
);


public record TodayHabitDto(
    long Id,
    string Name,
    string Color,
    bool IsCompleted,
    int Streak,
    TimeWindow? TimeWindow
);