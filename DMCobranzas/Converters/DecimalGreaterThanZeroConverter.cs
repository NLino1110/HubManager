using System.Globalization;

namespace DMCobranzas.Converters
{
    public class DecimalGreaterThanZeroConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                decimal d => d > 0,
                double dbl => dbl > 0,
                float f => f > 0,
                int i => i > 0,
                long l => l > 0,
                _ => false
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
