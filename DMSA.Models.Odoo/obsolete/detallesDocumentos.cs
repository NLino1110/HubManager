using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Models
{

    //public class Rootobject
    //{
    //    public string CODEMPRESA { get; set; }
    //    public string CODCLIENTE { get; set; }
    //    public string REFERENCIA { get; set; }
    //    public string VENDEDOR { get; set; }
    //    public string NUMCUOTA { get; set; }
    //    public string NUMDOCUMENTO { get; set; }
    //    public string FECHAEMISION { get; set; }
    //    public string DIASFEMISION { get; set; }
    //    public string VALORCUOTA { get; set; }
    //    public string VALORCHEQUE { get; set; }
    //    public string VALORSALDO { get; set; }
    //    public int ORDENAMIENTO { get; set; }
    //    public string VALORXAPLICAR { get; set; }
    //    public string BLOQUEADO { get; set; }
    //}


    public class detallesDocumentos
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        //public int ROWID { get; set; }
        
        public int CODEMPRESA { get; set; }
        public int CODCLIENTE { get; set; }
        public string NUMDOCUMENTO { get; set; }
        public int VENDEDOR { get; set; }
        public string FECHAEMISION { get; set; }
        public int DIASFEMISION { get; set; }
        public decimal VALORCHEQUE { get; set; }
        public int ORDENAMIENTO { get; set; }

        public string REFERENCIA { get; set; }
        public decimal VALORXAPLICAR { get; set; }
        public string BLOQUEADO { get; set; }
        public int NUMCUOTA { get; set; }
        public decimal VALORCUOTA { get; set; }
        public decimal VALORSALDO { get; set; }
        public decimal CANTIDADDEVUELTA { get; set; }
        public decimal CANTIDAD { get; set; }
        //NOTMAPPED
        //[NotMapped]
        //[IgnoreDataMember]
        //public string? ID { get; set; }
        //[NotMapped]
        //[IgnoreDataMember]
        //public string NOMBREUSUARIO { get; set; }
        //[NotMapped]
        //public string EMAILCLIENTE { get; set; }
        //[NotMapped]
        //public string CERRADO { get; set; }
    }

    public class detallesDocumentos_forSend
    {
        public string REFERENCIA { get; set; }
        public decimal VALORXAPLICAR { get; set; }
        public string BLOQUEADO { get; set; }
        public string NUMCUOTA { get; set; }
        public decimal VALORCUOTA { get; set; }
        public decimal VALORSALDO { get; set; }
        public decimal CANTIDADDEVUELTA { get; set; }
        public decimal CANTIDAD { get; set; }        
    }
}
