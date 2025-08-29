using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA.Models
{
    public class CobReciboDet
    {
        public int? iddetalle { get; set; }
        public string iddocumento { get; set; }
        public string idformapago { get; set; }
        public string descformapago { get; set; }
        public string idbanco { get; set; }
        public string descbanco { get; set; }
        public string idtarjeta { get; set; }
        public string desctarjeta { get; set; }
        public string numero_cheque { get; set; }
        public string cta_cheque { get; set; }
        public string idctacia { get; set; }
        public string descctacia { get; set; }
        public string numerodeposito { get; set; }
        public string emisor { get; set; }
        public string tipocheque { get; set; }
        public string fcobrocheque { get; set; }
        public string idindicador { get; set; }
        public string descindicador { get; set; }
        public string numero_lote { get; set; }
        public string numero_tarjeta { get; set; }
        public string numero_retencion { get; set; }
        public string valor { get; set; }
    }
}
