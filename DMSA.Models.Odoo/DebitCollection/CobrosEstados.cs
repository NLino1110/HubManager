using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DebitCollection
{
    public static class CobrosEstados
    {
        public static string PENDIENTE = "PENDIENTE";
        public const string LegacyEnviando = "ENVIANDO";
        public static string EN_PROCESO = "EN PROCESO";
        public static string ENVIANDO = EN_PROCESO;
        public static string ERROR = "ERROR";
        public static string PROCESADO = "PROCESADO";
        public static string CANCELADO = "CANCELADO";
        public static string APLICADO = "APLICADO";

        public static bool IsEnProceso(string paymentStatus) =>
            string.Equals(paymentStatus, EN_PROCESO, StringComparison.OrdinalIgnoreCase)
            || string.Equals(paymentStatus, LegacyEnviando, StringComparison.OrdinalIgnoreCase);

        public static string GetDisplayStatus(string paymentStatus) =>
            IsEnProceso(paymentStatus) ? EN_PROCESO : paymentStatus ?? string.Empty;

        public static bool CanEdit(string paymentStatus) =>
            string.Equals(paymentStatus, PENDIENTE, StringComparison.OrdinalIgnoreCase)
            || string.Equals(paymentStatus, ERROR, StringComparison.OrdinalIgnoreCase);

        public static bool CanSync(string paymentStatus) =>
            string.Equals(paymentStatus, PENDIENTE, StringComparison.OrdinalIgnoreCase)
            || IsEnProceso(paymentStatus);
    }
}
