using Cadence.CLI.Commands;
using Spectre.Console;
using Spectre.Console.Cli;

string[] effectiveArgs = args.Length == 0 ? ["today"] : args;

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("cx");
    config.SetExceptionHandler((ex, _) =>
    {
        AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
        return 1;
    });

    config.AddCommand<TodayCommand>("today")
        .WithDescription("Show and toggle today's habits (default)");

    config.AddCommand<DoneCommand>("done")
        .WithDescription("Mark a habit done  ·  cx done [[name]] [[--undo]]");

    config.AddCommand<WeekCommand>("week")
        .WithDescription("Show habit heatmap  ·  cx week <name> [[--weeks 8]]");

    config.AddBranch("habit", h =>
    {
        h.AddCommand<AddHabitCommand>("add")
            .WithDescription("Create a habit  ·  cx habit add <name>");
        h.AddCommand<ArchiveHabitCommand>("archive")
            .WithDescription("Archive a habit");
        h.AddCommand<ListHabitsCommand>("list")
            .WithDescription("List habits with scheduled days and streak");
        h.AddCommand<ColorsCommand>("colors")
            .WithDescription("List available color names");
    });
});

return app.Run(effectiveArgs);
