using System.Reflection;

namespace BeebTech.Controls.Converters;
public static class UraniumConverters
{
    public static StringIsNotNullOrEmptyConverter StringIsNotNullOrEmptyConverter { get; } = new();
}
