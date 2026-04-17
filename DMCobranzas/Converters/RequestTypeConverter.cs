using System.Globalization;

namespace DMCobranzas.Converters
{
    public class RequestTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            if (value == null)
                return string.Empty;

            var name = value.ToString();

            if(name == "invoice")
                return "Una factura";
            else if(name == "invoices")
                return "Varias facturas";
            else
                return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
