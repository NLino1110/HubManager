using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Models
{
    public class FacNotaCreditoDet
    {
        //[PrimaryKey]
        //[NotNull]
        
        public int line_id { get; set; }
        public int NUMCMPRVENTA{ get; set; }
        public int NUMCMPRVENTADET{ get; set; }
        public string CODARTICULO { get; set; }
        public string ARTICULO{ get; set; }
        public decimal COSTO { get; set; }
        public decimal CANTIDAD { get; set; }
        public decimal PRECIO { get; set; }
        public decimal SUBTOTAL { get; set; }
        public decimal DESCUENTO { get; set; }
        public decimal PORCDESCUENTO { get; set; }
        public decimal IMPUESTO { get; set; }
        public decimal PORCIMPUESTO { get; set; }
        public decimal TOTAL { get; set; }
        public decimal CANTIDADDEVUELTA { get; set; }
        
        //nuevos
        public string display_type { get; set; }

        //SE OMITIRAN
        //public string ESPREMIOOPROMOCION{ get; set; }
        //public string NUMCMPRVENTADETAPLICA{ get; set; }        
        //public decimal CANTIDADNOCONFORME { get; set; }
        //[PrimaryKey]
        //[NotNull]
        //public string CODAGENCIA { get; set; }
        //[PrimaryKey]
        //[NotNull]
        //public string CODTIPOCMPR { get; set; }



        //[Ignore]        
        //public string? NUMDOCUMENTO { get; set; }

        //[Ignore]
        //public string? FECHAREGISTRO { get; set; }

    }


    public class FacNotaCreditoDetAuxiliar : FacNotaCreditoDet
    {
        ////[PrimaryKey]
        ////[NotNull]
        //public string CODAGENCIA { get; set; }
        ////[PrimaryKey]
        ////[NotNull]
        //public string CODTIPOCMPR { get; set; }
        ////[PrimaryKey]
        ////[NotNull]
        //public string NUMCMPRVENTA { get; set; }
        //public string NUMCMPRVENTADET { get; set; }
        //public string ARTICULO { get; set; }
        //public string COSTO { get; set; }
        //public string CANTIDAD { get; set; }
        //public string PRECIO { get; set; }
        //public string SUBTOTAL { get; set; }
        //public string DESCUENTO { get; set; }
        //public string PORCDESCUENTO { get; set; }
        //public string IMPUESTO { get; set; }
        //public string PORCIMPUESTO { get; set; }
        //public string TOTAL { get; set; }
        //public string ESPREMIOOPROMOCION { get; set; }
        //public string CANTIDADDEVUELTA { get; set; }
        //public string NUMCMPRVENTADETAPLICA { get; set; }
        //public string CODARTICULO { get; set; }
        //public string CANTIDADNOCONFORME { get; set; }

        public string? CANTIDADDEVUELTAMBW { get; set; }
        public string? NUMDOCUMENTO { get; set; }
        public string FECHAREGISTRO { get; set; }
        //[Ignore]
        public string? BLOQUEADO { get; set; }
        public decimal reconcile_amount { get; set; }
    }
}
