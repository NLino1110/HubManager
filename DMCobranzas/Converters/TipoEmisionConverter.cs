using DMSA.Models.Odoo.DMCobranzas;
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
        private static readonly List<AppParameter> _tipoEmision = new()
        {
            new AppParameter { name = "transfer", value = "Trasferencia" },
            new AppParameter { name = "deposito", value = "Depósito" },
            new AppParameter { name = "cash", value = "Efectivo" },
            new AppParameter { name = "check_day", value = "Cheque Día" },
            new AppParameter { name = "check", value = "Cheque PF" },
            new AppParameter { name = "credit_card", value = "Tarjeta Crédito" },
            new AppParameter { name = "otros", value = "Otros" }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            var name = value.ToString();

            var result = _tipoEmision.Find(x => x.name == name);

            return result?.value ?? name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
