using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Models
{
    [Obsolete]
    public class CobCarteraDet
    {
        public int CODEMPRESA  { get; set; }
        public int CODCLIENTE { get; set; }
        public string REFERENCIA { get; set; }
        public int VENDEDOR { get; set; }
        public int NUMCUOTA { get; set; }
        public string NUMDOCUMENTO { get; set; }
        public string FECHAEMISION { get; set; }
        public int DIASFEMISION { get; set; }
        public decimal VALORCUOTA { get; set; }
        public decimal VALORCHEQUE { get; set; }
        public decimal VALORSALDO { get; set; }
        public int ORDENAMIENTO { get; set; }
        public decimal VALORXAPLICAR { get; set; }

        [Ignore]
        public string? BLOQUEADO { get; set; }
    }
}
