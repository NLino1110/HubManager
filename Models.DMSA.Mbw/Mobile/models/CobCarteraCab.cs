using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA.Models
{
    public class CobCarteraCab
    {
        public string CODEMPRESA { get; set; }
        public string CODCLIENTE { get; set; }
        public string NOMBRECLIENTE { get; set; }
        public string EMAILCLIENTE { get; set; }
        public string CODIGOVENDEDOR { get; set; }
        public string USUARIOVENDEDOR { get; set; }

        //TODO: Convertir a moneda
        public string VENCIDO { get; set; }
        public string XVENCER { get; set; }
        public string AFAVOR { get; set; }
        public string TOTAL { get; set; }
    }
}
