namespace Cadence.API.Data.ValueObjects;

public record TimeWindow(TimeOnly Start, TimeOnly End)
{
    public TimeOnly End { get; init; } = End > Start
        ? End
        : throw new ArgumentException("End must be after Start.");
};