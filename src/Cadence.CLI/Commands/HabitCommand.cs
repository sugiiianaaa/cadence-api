using Cadence.CLI.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Cadence.CLI.Commands;

// cx habit add <name>
sealed class AddHabitCommand : AsyncCommand<AddHabitCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<name>")]
        public string Name { get; init; } = "";

        [CommandOption("-c|--color <color>")]
        [Description("Hex color, e.g. #ff5f00 (default: #6c757d).")]
        [DefaultValue("#6c757d")]
        public string Color { get; init; } = "#6c757d";

        // TODO: add --days option to specify scheduled days, e.g. --days mon,tue,wed
        // For now defaults to every day
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var client = CadenceClient.FromEnv();

        // TODO: prompt for scheduled days if not passed via --days
        var allDays = Enum.GetValues<DayOfWeek>().ToList();
        
        var request = new CreateHabitRequest(
            settings.Name,
            Description: null,
            settings.Color,
            TimeWindow: null, // will fail until API accepts null
            allDays);

        throw new NotImplementedException(
            "API CreateHabitInputDto requires TimeWindow. Make it nullable there first, then remove this throw.");
    }
}

// cx habit archive
sealed class ArchiveHabitCommand : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        var client = CadenceClient.FromEnv();

        // TODO: GetAllHabitsAsync doesn't exist yet — implement CadenceClient.GetAllHabitsAsync()
        // by checking the GET /api/habits response shape in GetHabits.cs
        var habits = await client.GetTodayAsync(); // placeholder — only shows today's habits

        if (habits.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No habits to archive.[/]");
            return 0;
        }

        var habit = AnsiConsole.Prompt(
            new SelectionPrompt<TodayHabitDto>()
                .Title("Archive which habit? [red](irreversible)[/]")
                .UseConverter(h => h.Name)
                .AddChoices(habits));

        var confirmed = AnsiConsole.Confirm($"Archive [bold]{habit.Name}[/]?", defaultValue: false);
        if (!confirmed) return 0;

        await client.ArchiveHabitAsync(habit.Id);
        AnsiConsole.MarkupLine($"[grey]{habit.Name} archived.[/]");
        return 0;
    }
}
