using System.Diagnostics;
using System.Globalization;

namespace DMCobranzas.Converters
{
    public class DecimalToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {                
                if (value == null)
                    return "";
                                
                if (value is string str)
                    return str;
                                
                if (value is decimal dec)
                {                    
                    return dec.ToString(culture);
                }

                return value.ToString();
            }
            catch
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {                
                var str = value?.ToString();
                                
                if (string.IsNullOrWhiteSpace(str))
                    return 0m;
                                
                str = str.Replace(",", culture.NumberFormat.NumberDecimalSeparator)
                         .Replace(".", culture.NumberFormat.NumberDecimalSeparator);
                                
                if (decimal.TryParse(str, NumberStyles.Any, culture, out var result))
                {                    
                    return result;
                }

                return 0m;
            }
            catch
            {
                return 0m;
            }
        }
    }
}