using Cadence.CLI.Client;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Cadence.CLI.Commands;

internal sealed class TodayCommand : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        var client = CadenceClient.FromEnv();
        var habits = await client.GetTodayAsync();

        if (habits.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No habits scheduled for today.[/]");
            return 0;
        }

        var prompt = new MultiSelectionPrompt<TodayHabitDto>()
            .Title("Today's habits [grey](space to toggle, enter to save)[/]")
            .NotRequired()
            .UseConverter(h => FormatHabit(h));

        foreach (var habit in habits)
        {
            prompt.AddChoice(habit);
            if (habit.IsCompleted)
                prompt.Select(habit);
        }

        var selected = AnsiConsole.Prompt(prompt);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var tasks = habits.Select(h =>
        {
            bool shouldBeCompleted = selected.Contains(h);
            if (shouldBeCompleted == h.IsCompleted)
                return Task.CompletedTask;
            return client.SetCompletionAsync(h.Id, today, shouldBeCompleted);
        });

        await Task.WhenAll(tasks);

        // TODO: show a summary line after saving, e.g. "3/4 done · 🔥 pushups streak 7"
        AnsiConsole.MarkupLine("[green]Saved.[/]");
        return 0;
    }

    private static string FormatHabit(TodayHabitDto h)
    {
        string window = h.TimeWindow is not null
            ? $" [grey]{h.TimeWindow.Start:HH\\:mm}–{h.TimeWindow.End:HH\\:mm}[/]"
            : "";
        string streak = h.Streak > 0 ? $" [grey]·{h.Streak}[/]" : "";
        return $"[{h.Color}]●[/] {h.Name}{window}{streak}";
    }
}
