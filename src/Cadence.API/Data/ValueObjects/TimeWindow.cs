namespace Cadence.API.Data.ValueObjects;

public record TimeWindow
{
    public TimeOnly Start { get; init; }
    public TimeOnly End { get; init; }
    
    public TimeWindow(TimeOnly start, TimeOnly end)
    {
        if(end <= start)
            throw new ArgumentOutOfRangeException(nameof(end), "End must after start time");
        
        (Start, End) = (start, end);
    }
}