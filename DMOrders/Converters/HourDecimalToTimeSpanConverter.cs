using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Converters
{
    public class HourDecimalToTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is decimal h)
            {
                var hours = (int)Math.Floor(h);
                var minutes = (int)Math.Round((h - hours) * 60);
                // fecha base: hoy, o pásala en ConverterParameter (yyyy-MM-dd) si quieres otra
                var baseDate = DateTime.Today;
                return baseDate.Date.AddHours(hours).AddMinutes(minutes);
            }
            return DateTime.Today;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }

}
