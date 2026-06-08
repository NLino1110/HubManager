using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DMCobranzas;

namespace DMCobranzas.Settings.helpers
{
    public class ParseTool
    {
        static public double StringToDouble_(string StringValue)
        {
            // Intenta convertir el texto a double utilizando la cultura especificada.
            if (double.TryParse(StringValue, NumberStyles.Number, App.Session.ApplicationCultureInfo, out double valorDouble))
            {
                return valorDouble; // Si la conversión tiene éxito, devuelve el valor double.
            }
            else
            {
                return double.NaN; // Si no se puede convertir, devuelve un valor indicando error (por ejemplo, double.NaN).
            }
        }

        static public string StringValueFix(string texto)
        {
            texto = texto.Replace(",",".");
            return texto;
        }

        static public double StringToDouble(string texto, CultureInfo cultura = null)
        {
            if(string.IsNullOrEmpty(texto))
            {
                texto = "0.00";
            }

            // Primero, intentamos analizar el valor con la cultura en-US (punto como decimal).
            if (double.TryParse(texto, NumberStyles.Any, App.Session.ApplicationCultureInfo, out double valorDouble))
            {
                return valorDouble;
            }
            // Si la conversión no tiene éxito, intentamos con la cultura en-ES (coma como decimal).
            else if (double.TryParse(texto, NumberStyles.Any, CultureInfo.GetCultureInfo("es-ES"), out valorDouble))
            {
                return valorDouble;
            }
            // Si aún no se puede analizar, se lanza una excepción o se devuelve NaN, dependiendo de tus preferencias.
            else
            {
                throw new FormatException("El texto no tiene un formato válido para convertir a double: " + texto);
            }
        }

        static public decimal StringToDecimal(string texto, CultureInfo cultura = null)
        {
            if (decimal.TryParse(texto, out decimal numero))
            {
                return numero;
            }
            else
            {
                return 0; // Devuelve el valor original si no es un número válido
            }
        }

        public static string ConvertirAMoneda(string valor)
        {
            if (decimal.TryParse(valor, out decimal numero))
            {
                return numero.ToString(App.Session.ApplicationCultureInfo);
            }
            else
            {
                return valor; // Devuelve el valor original si no es un número válido
            }
        }

        //public static string CleanServerMessage_v1_old(string message, bool returnAll)
        //{
        //    string patron = @"\('([^']+)'\)";
                        
        //    Regex regex = new Regex(patron);
                        
        //    MatchCollection coincidencias = regex.Matches(message);

        //    string full_return = "";
            
        //    foreach (Match coincidencia in coincidencias)
        //    {
        //        if (!returnAll)
        //            return coincidencia.Groups[1].Value;
        //        //Console.WriteLine(coincidencia.Groups[1].Value);
        //        full_return += coincidencia.Groups[1].Value;
        //    }

        //    return full_return;
        //}

        public static string CleanServerMessage_v1(string message, bool returnAll)
        {
            string patron = @"\('([^']+)'\)";
            Regex regex = new Regex(patron);

            MatchCollection coincidencias = regex.Matches(message);

            if (coincidencias.Count == 0)
                return message; // 👈 fallback

            string full_return = "";

            foreach (Match coincidencia in coincidencias)
            {
                if (!returnAll)
                    return coincidencia.Groups[1].Value;

                full_return += coincidencia.Groups[1].Value;
            }

            return full_return;
        }
    }
}
