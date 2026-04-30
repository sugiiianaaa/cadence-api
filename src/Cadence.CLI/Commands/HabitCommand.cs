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
        var client = CadenceClient.FromEnv();

        var request = new CreateHabitRequest(
            settings.Name,
            settings.Color,
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
        [Description("Hex color, e.g. #ff5f00 (default: #6c757d).")]
        [DefaultValue("#6c757d")]
        public string Color { get; init; } = "#6c757d";

        [CommandOption("-d|--days <days>")]
        [Description("Comma-separated days 0–6 (Sun=0), e.g. 0,1,2")]
        [TypeConverter(typeof(DayOfWeekListConverter))]
        public List<DayOfWeek> Days { get; init; } = [];

        [CommandOption("-s|--start-time <start>")]
        [Description("Start time, e.g. 06:00 or 06.00")]
        [TypeConverter(typeof(TimeOnlyConverter))]
        public TimeOnly StartTime { get; init; } = new TimeOnly(6, 0);

        [CommandOption("-e|--end-time <end>")]
        [Description("End time, e.g. 07:00 or 07.00")]
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
