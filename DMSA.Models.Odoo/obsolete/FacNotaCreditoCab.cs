using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Models
{
    public class FacNotaCreditoCab
    {
        [PrimaryKey]
        public int NUMCMPRVENTA { get; set; }
        public int CODAGENCIA { get; set; }
        public int CODEMPRESA{ get; set; }
        public int CODCLIENTE{ get; set; }
        public string VAT { get; set; }
        public string DATOS_CLIENTE{ get; set; }
        public string TIPOCLIENTE{ get; set; }        
        public string CODTIPOCMPR{ get; set; }        
        public string NUMDOCUMENTO{ get; set; }
        public string VENDEDOR{ get; set; }
        public string SUBTOTAL{ get; set; }
        public decimal DESCUENTO { get; set; }
        public decimal IMPUESTO { get; set; }
        public string TOTAL{ get; set; }
        public string FECHAREGISTRO{ get; set; }

        //Campos que no se van a utilizar 
        //public string CODAGENCIACXC{ get; set; }
        //public string CODTIPOCMPRCXC{ get; set; }
        //public string NUMCXCDOCUMENTO{ get; set; }
        //public DateTime FECHAVCTO{ get; set; }
        //public decimal VALORDOC { get; set; }
        //public decimal SALDODOC { get; set; }
    }




}
