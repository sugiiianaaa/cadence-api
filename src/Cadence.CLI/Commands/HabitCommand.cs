using System.ComponentModel;
using Cadence.CLI.Client;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Cadence.CLI.Commands;

// cx habit add <name>
internal sealed class AddHabitCommand : AsyncCommand<AddHabitCommand.Settings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (settings.Days.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]--days is required.[/] Example: [grey]--days 1,2,3,4,5[/] (Mon=1, Sun=0)");
            return 1;
        }

        string? hex = Colors.Resolve(settings.Color);
        if (hex is null)
        {
            AnsiConsole.MarkupLine($"[red]Unknown color '{settings.Color}'.[/] Available: [grey]{Colors.ListHelp()}[/]");
            return 1;
        }

        var client = CadenceClient.FromEnv();

        var request = new CreateHabitRequest(
            settings.Name,
            hex,
            new TimeWindowDto(settings.StartTime, settings.EndTime),
            settings.Days);

        long habitId = await client.CreateHabitAsync(request);

        AnsiConsole.MarkupLine($"[green]Created habit #{habitId}[/]");

        return 0;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<name>")]
        public string Name { get; init; } = "";

        [CommandOption("-c|--color <color>")]
        [Description("Color name (red, green, blue…) or hex (#rrggbb). Run 'cx habit colors' to list all.")]
        [DefaultValue("grey")]
        public string Color { get; init; } = "grey";

        [CommandOption("-d|--days <days>")]
        [Description("Comma-separated days 0–6 (Sun=0, Mon=1). Required. Example: 1,2,3,4,5")]
        [TypeConverter(typeof(DayOfWeekListConverter))]
        public List<DayOfWeek> Days { get; init; } = [];

        [CommandOption("-s|--start-time <start>")]
        [Description("Start time, e.g. 06:00")]
        [TypeConverter(typeof(TimeOnlyConverter))]
        public TimeOnly StartTime { get; init; } = new TimeOnly(6, 0);

        [CommandOption("-e|--end-time <end>")]
        [Description("End time, e.g. 07:00")]
        [TypeConverter(typeof(TimeOnlyConverter))]
        public TimeOnly EndTime { get; init; } = new TimeOnly(6, 30);
    }
}

// cx habit archive
internal sealed class ArchiveHabitCommand : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        var client = CadenceClient.FromEnv();

        List<GetHabitOutputDto> habits = await client.GetAllHabitsAsync();

        if (habits.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No habits to archive.[/]");
            return 0;
        }

        GetHabitOutputDto habit = AnsiConsole.Prompt(
            new SelectionPrompt<GetHabitOutputDto>()
                .Title("Archive which habit? [red](irreversible)[/]")
                .UseConverter(h => h.Name)
                .AddChoices(habits));

        bool confirmed = AnsiConsole.Confirm($"Archive [bold]{habit.Name}[/]?", false);
        if (!confirmed)
            return 0;

        await client.ArchiveHabitAsync(habit.Id);
        AnsiConsole.MarkupLine($"[grey]{habit.Name} archived.[/]");
        return 0;
    }
}

// cx habit list
internal sealed class ListHabitsCommand : AsyncCommand
{
    private static readonly string[] DayOrder =
        ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        var client = CadenceClient.FromEnv();
        List<GetHabitOutputDto> habits = await client.GetAllHabitsAsync();

        if (habits.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No habits yet. Add one with: cx habit add <name>[/]");
            return 0;
        }

        foreach (GetHabitOutputDto habit in habits)
        {
            string days = FormatDays(habit.ScheduledDays);
            string streak = habit.CurrentStreak > 0 ? $"  [grey]streak {habit.CurrentStreak}[/]" : "";
            AnsiConsole.MarkupLine($"[{habit.Color}]●[/] {habit.Name}  [grey]{days}[/]{streak}");
        }

        return 0;
    }

    private static string FormatDays(List<string> days)
    {
        if (days.Count == 7)
            return "daily";
        IOrderedEnumerable<string> sorted = days.OrderBy(d => Array.IndexOf(DayOrder, d));
        return string.Join(" ", sorted.Select(d => d[..3]));
    }
}

// cx habit colors
internal sealed class ColorsCommand : Command
{
    public override int Execute(CommandContext context)
    {
        AnsiConsole.MarkupLine($"Available colors: [grey]{Colors.ListHelp()}[/]  (or use hex: #rrggbb)");
        return 0;
    }
}
