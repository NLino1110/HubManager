using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA.Models
{
    public class CobCarteraDet
    {
        public string CODEMPRESA  { get; set; }
        public string CODCLIENTE { get; set; }
        public string REFERENCIA { get; set; }
        public string VENDEDOR { get; set; }
        public string NUMCUOTA { get; set; }
        public string NUMDOCUMENTO { get; set; }
        public string FECHAEMISION { get; set; }
        public string DIASFEMISION { get; set; }
        public string VALORCUOTA { get; set; }
        public string VALORCHEQUE { get; set; }
        public string VALORSALDO { get; set; }
        public string ORDENAMIENTO { get; set; }
        public string VALORXAPLICAR { get; set; }

        [Ignore]
        public string? BLOQUEADO { get; set; }
    }
}
