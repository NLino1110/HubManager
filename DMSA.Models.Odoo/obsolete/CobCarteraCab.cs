using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Models
{
    [Obsolete]
    public class CobCarteraCab
    {
        public int CODEMPRESA { get; set; }
        public int CODCLIENTE { get; set; }
        public string VAT { get; set; }
        public string NOMBRECLIENTE { get; set; }
        public string EMAILCLIENTE { get; set; }
        public string CODIGOVENDEDOR { get; set; }
        public string USUARIOVENDEDOR { get; set; }

        //TODO: Convertir a moneda
        public decimal VENCIDO { get; set; }
        public decimal XVENCER { get; set; }
        public decimal AFAVOR { get; set; }
        public decimal TOTAL { get; set; }

        [Ignore]
        public CobCarteraDet[] DETALLE { get; set; }
    }
}
