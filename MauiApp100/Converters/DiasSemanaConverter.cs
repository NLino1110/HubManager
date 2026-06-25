using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Converters
{
    public class DiasSemanaConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 7)
                return string.Empty;

            string[] nombres = { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
            var diasActivos = new List<string>();

            for (int i = 0; i < 7; i++)
            {
                if (values[i] is bool b && b)
                    diasActivos.Add(nombres[i]);
            }

            return diasActivos.Count > 0 ? string.Join(",", diasActivos) : "—";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
