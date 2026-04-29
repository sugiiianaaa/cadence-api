using Cadence.CLI.Client;
using Spectre.Console;

namespace Cadence.CLI.Rendering;

internal static class WeekGrid
{
    // Unicode blocks ordered by density
    private static readonly string[] Blocks = [" ", "░", "▒", "▓", "█"];

    public static void Render(List<HeatmapDayDto> days)
    {
        if (days.Count == 0)
            return;

        // Pad to full week boundary at the start
        var first = days[0].Date;
        int startPad = (int)first.DayOfWeek; // 0 = Sunday
        var padded = Enumerable
            .Repeat<HeatmapDayDto?>(null, startPad)
            .Concat(days.Select(d => (HeatmapDayDto?)d))
            .ToList();

        // Fill to full weeks
        while (padded.Count % 7 != 0)
            padded.Add(null);

        // Header
        AnsiConsole.MarkupLine("[grey]Sun Mon Tue Wed Thu Fri Sat[/]");

        for (int i = 0; i < padded.Count; i += 7)
        {
            var week = padded.Skip(i).Take(7);
            var cells = week.Select(d => d is null ? "    " : Cell(d)).ToList();
            AnsiConsole.MarkupLine(string.Join(" ", cells));
        }

        // Legend
        AnsiConsole.MarkupLine("[grey] ░ ▒ ▓ █  = 0% → 100%[/]");
    }

    private static string Cell(HeatmapDayDto day)
    {
        if (day.Total == 0)
            return "[grey]  · [/]";

        double ratio = (double)day.Completed / day.Total;
        string block = ratio switch
        {
            0 => $"[red]{Blocks[0]}[/]",
            <= 0.33 => $"[red]{Blocks[1]}[/]",
            <= 0.66 => $"[yellow]{Blocks[2]}[/]",
            < 1 => $"[green]{Blocks[3]}[/]",
            _ => $"[green]{Blocks[4]}[/]",
        };

        // TODO: tweak colours to your taste — Spectre supports full hex: [#ff5f00]
        return $" {block} ";
    }
}
