using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Converters
{
    public class MiscEstadoToActivoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value?.ToString()?.Trim().ToLower();

            return str switch
            {
                "activo" => "ACTIVO",
                "inactivo" => "INACTIVO",
                "false" or null or "" => "-",
                _ => "-"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
