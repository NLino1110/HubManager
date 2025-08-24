using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using DMOrders.Models;

namespace DMOrders.Converters
{
    

    internal class StreakToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Streak s)
            {
                return s.Result == Result.Won
                    ? Colors.Green.AddLuminosity(s.NumStreak / 30F)
                    : Colors.Red.AddLuminosity(-s.NumStreak / 30F);
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
