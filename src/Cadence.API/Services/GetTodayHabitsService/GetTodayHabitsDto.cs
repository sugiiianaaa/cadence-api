public record GetTodayHabitsDto(
    List<TodayHabitDto> TodayHabits
);


public record TodayHabitDto(
    long Id,
    string Name,
    string Color,
    bool IsCompleted,
    int Streak,
    TimeOnly StartRangeTime,
    TimeOnly EndRangeTime
);