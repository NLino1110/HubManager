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
                "PENDIENTE" => "Pendiente",
                "PARCIAL" => "Parcial",
                "COMPLETA" => "Completa",
                "ERROR" => "Error",
                "SINCRONIZADO" or "SINCRONIZADA" => "Sincronizada",
                _ when up.StartsWith("PARCIAL (") => s,
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