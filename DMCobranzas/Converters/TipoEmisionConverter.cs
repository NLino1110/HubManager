using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.StaticData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCobranzas.Converters
{
    public class TipoEmisionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<AppParameter> _tipoEmision = TipoEmision.data;

            if (value == null)
                return string.Empty;

            var name = value.ToString();

            var result = _tipoEmision.Find(x => x.code == name);

            return result?.name ?? name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
