using Cadence.CLI.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Cadence.CLI.Commands;

sealed class DoneCommand : AsyncCommand<DoneCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[name]")]
        [Description("Habit name (partial match). Omit to pick from a list.")]
        public string? Name { get; init; }

        [CommandOption("--undo")]
        [Description("Unmark instead of mark.")]
        public bool Undo { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var client = CadenceClient.FromEnv();
        var habits = await client.GetTodayAsync();

        if (habits.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No habits scheduled for today.[/]");
            return 0;
        }

        TodayHabitDto habit;

        if (settings.Name is not null)
        {
            var matches = habits
                .Where(h => h.Name.Contains(settings.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            habit = matches.Count switch
            {
                0 => throw new InvalidOperationException($"No habit matches '{settings.Name}'."),
                1 => matches[0],
                _ => PickFromList(matches, "Multiple matches — pick one:"),
            };
        }
        else
        {
            var candidates = settings.Undo
                ? habits.Where(h => h.IsCompleted).ToList()
                : habits.Where(h => !h.IsCompleted).ToList();

            if (candidates.Count == 0)
            {
                var msg = settings.Undo ? "Nothing marked done yet." : "Everything's done for today.";
                AnsiConsole.MarkupLine($"[grey]{msg}[/]");
                return 0;
            }

            habit = candidates.Count == 1
                ? candidates[0]
                : PickFromList(candidates, "Pick a habit:");
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        await client.SetCompletionAsync(habit.Id, today, !settings.Undo);

        var verb = settings.Undo ? "Unmarked" : "Done";
        AnsiConsole.MarkupLine($"[green]{verb}:[/] {habit.Name}");
        return 0;
    }

    private static TodayHabitDto PickFromList(List<TodayHabitDto> habits, string title)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<TodayHabitDto>()
                .Title(title)
                .UseConverter(h => h.Name)
                .AddChoices(habits));
    }
}
