using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Models
{
    public class SolicitudesNC
    {
        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        public int ID { get; set; }

        public string IDNOTACREDITO { get; set; }
        public int CODEMPRESA{ get; set; }
        public string TIPONOTACREDITO{ get; set; }        
        public DateTime FECHA{ get; set; }
        
        public int uid { get; set; }

        //public string CODUSUARIO{ get; set; }
        //public string CLIENTE{ get; set; }
        public string DETALLESNC{ get; set; }
        public string CODESTADO{ get; set; }
        public string OBSERVACIONES{ get; set; }
        public string FECRESP_MWB{ get; set; }

        public int CODCLIENTE { get; set; }
        public string NOMBRECLIENTE { get; set; }

        [Ignore]
        public string? NOMBREUSUARIO { get; set; }
        [Ignore]
        public string? EMAILCLIENTE { get; set; }
        [Ignore]
        public string? CERRADO { get; set; }
        [Ignore]
        public bool? isGroup { get; set; }
    }
}
