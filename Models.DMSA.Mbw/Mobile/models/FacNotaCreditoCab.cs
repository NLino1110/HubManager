using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA.Models
{
    public class FacNotaCreditoCab
    {
        public string CODEMPRESA{ get; set; }
        public string CODCLIENTE{ get; set; }
        public string DATOS_CLIENTE{ get; set; }
        public string TIPOCLIENTE{ get; set; }
        public string CODAGENCIA{ get; set; }
        public string CODTIPOCMPR{ get; set; }
        public string NUMCMPRVENTA{ get; set; }
        public string NUMDOCUMENTO{ get; set; }
        public string VENDEDOR{ get; set; }
        public string SUBTOTAL{ get; set; }
        public string DESCUENTO{ get; set; }
        public string IMPUESTO{ get; set; }
        public string TOTAL{ get; set; }
        public string FECHAREGISTRO{ get; set; }
        public string CODAGENCIACXC{ get; set; }
        public string CODTIPOCMPRCXC{ get; set; }
        public string NUMCXCDOCUMENTO{ get; set; }
        public string FECHAVCTO{ get; set; }
        public string VALORDOC{ get; set; }
        public string SALDODOC{ get; set; }
    }
}
