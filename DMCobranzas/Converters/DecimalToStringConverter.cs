using System.Globalization;

namespace DMCobranzas.Converters
{
    public class DecimalToStringConverter : IValueConverter
    {
        public static decimal ParseAmount(string? text, CultureInfo? culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;
            return (decimal)new DecimalToStringConverter().ConvertBack(text, typeof(decimal), null, culture)!;
        }

        public static string FormatAmount(decimal value, CultureInfo? culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;
            return (string)new DecimalToStringConverter().Convert(value, typeof(string), null, culture)!;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            if (value is decimal dec)
                return dec.ToString("N2", culture);

            if (value is double dbl)
                return ((decimal)dbl).ToString("N2", culture);

            if (decimal.TryParse(value.ToString(), NumberStyles.Number, culture, out var parsed))
                return parsed.ToString("N2", culture);

            return value.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value?.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(str))
                return 0m;

            str = str.Replace(" ", string.Empty);

            if (decimal.TryParse(str, NumberStyles.Number, culture, out var cultureResult))
                return cultureResult;

            var normalized = str.Replace(",", ".");
            if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariantResult))
                return invariantResult;

            return 0m;
        }
    }
}
