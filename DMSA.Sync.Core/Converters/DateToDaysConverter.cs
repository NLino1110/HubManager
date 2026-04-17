using System.Globalization;

namespace DMSA.Sync.Core.Converters
{
    public class DateToDaysConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime date || date == DateTime.MinValue)
                return "-";

            var dias = Math.Max(0, (DateTime.Now.Date - date.Date).Days);
            return dias;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
