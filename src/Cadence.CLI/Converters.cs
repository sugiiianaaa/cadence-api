using System.ComponentModel;
using System.Globalization;

namespace Cadence.CLI;

internal sealed class TimeOnlyConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string);

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        => TimeOnly.Parse(((string)value).Replace('.', ':'), CultureInfo.InvariantCulture);
}

internal sealed class DayOfWeekListConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string);

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        => ((string)value)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => (DayOfWeek)int.Parse(s, CultureInfo.InvariantCulture))
            .ToList();
}
