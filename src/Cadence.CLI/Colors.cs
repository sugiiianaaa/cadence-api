namespace Cadence.CLI;

internal static class Colors
{
    private static readonly Dictionary<string, string> Named = new(StringComparer.OrdinalIgnoreCase)
    {
        ["red"]    = "#e05252",
        ["orange"] = "#e07c52",
        ["yellow"] = "#e0c452",
        ["lime"]   = "#7de052",
        ["green"]  = "#52a852",
        ["teal"]   = "#52a8a8",
        ["sky"]    = "#52a8e0",
        ["blue"]   = "#5278e0",
        ["purple"] = "#9052e0",
        ["pink"]   = "#e052a8",
        ["brown"]  = "#8b5e3c",
        ["grey"]   = "#6c757d",
    };

    public static string? Resolve(string input)
    {
        if (input.StartsWith('#') && input.Length == 7)
            return input;
        return Named.TryGetValue(input, out string? hex) ? hex : null;
    }

    public static string ListHelp() =>
        string.Join("  ", Named.Keys);
}
