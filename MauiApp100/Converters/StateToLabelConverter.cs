using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace DMOrders.Converters
{
    public class StateToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            var s = value.ToString() ?? string.Empty;
            var up = s.Trim().ToUpperInvariant();

            return up switch
            {
                "ACTIVO" => "Activo",
                "SINCRONIZADO" or "SINCRONIZADA" => "Sincronizada",
               
                _ => CapitalizeFirstLetter(s)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        static string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            input = input.ToLowerInvariant();
            return char.ToUpperInvariant(input[0]) + input.Substring(1);
        }
    }
}