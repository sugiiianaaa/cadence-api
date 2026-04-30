using System.ComponentModel;
using Cadence.CLI.Client;
using Cadence.CLI.Rendering;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Cadence.CLI.Commands;

internal sealed class WeekCommand : AsyncCommand<WeekCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[name]")]
        [Description("Habit name (partial match) to show a single habit's grid.")]
        public string? Name { get; init; }

        [CommandOption("-w|--weeks <weeks>")]
        [Description("How many weeks to show (default: 8).")]
        [DefaultValue(8)]
        public int Weeks { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var client = CadenceClient.FromEnv();

        if (settings.Name is not null)
        {
            List<GetHabitOutputDto> habits = await client.GetAllHabitsAsync();
            var matches = habits
                .Where(h => h.Name.Contains(settings.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            GetHabitOutputDto habit = matches.Count switch
            {
                0 => throw new InvalidOperationException($"No habit matches '{settings.Name}'."),
                1 => matches[0],
                _ => AnsiConsole.Prompt(
                    new SelectionPrompt<GetHabitOutputDto>()
                        .Title("Multiple matches — pick one:")
                        .UseConverter(h => h.Name)
                        .AddChoices(matches)),
            };

            AnsiConsole.MarkupLine($"[bold]{habit.Name}[/]");
            WeekGrid.Render(BuildHabitHeatmap(habit, settings.Weeks));
        }
        else
        {
            List<HeatmapDayDto> heatmap = await client.GetHeatmapAsync(settings.Weeks);
            AnsiConsole.MarkupLine("[bold]All habits[/]");
            WeekGrid.Render(heatmap);
        }

        return 0;
    }

    private static List<HeatmapDayDto> BuildHabitHeatmap(GetHabitOutputDto habit, int weeks)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        DateOnly from = today.AddDays(-(weeks * 7 - 1));
        var scheduledDays = habit.ScheduledDays
            .Select(d => Enum.Parse<DayOfWeek>(d))
            .ToHashSet();

        var result = new List<HeatmapDayDto>(weeks * 7);
        for (DateOnly d = from; d <= today; d = d.AddDays(1))
        {
            int scheduled = scheduledDays.Contains(d.DayOfWeek) ? 1 : 0;
            int completed = habit.RecentCompletions.Contains(d) ? 1 : 0;
            result.Add(new HeatmapDayDto(d, completed, scheduled));
        }
        return result;
    }
}
