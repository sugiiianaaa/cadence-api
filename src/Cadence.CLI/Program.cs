using Cadence.CLI.Commands;
using Spectre.Console;
using Spectre.Console.Cli;

var effectiveArgs = args.Length == 0 ? ["today"] : args;

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
        .WithDescription("Mark a habit done  ·  cx done [name] [--undo]");

    config.AddCommand<WeekCommand>("week")
        .WithDescription("Show weekly heatmap  ·  cx week [--weeks 8]");

    config.AddBranch("habit", h =>
    {
        h.AddCommand<AddHabitCommand>("add")
            .WithDescription("Create a habit  ·  cx habit add <name>");
        h.AddCommand<ArchiveHabitCommand>("archive")
            .WithDescription("Archive a habit");
    });
});

return app.Run(effectiveArgs);