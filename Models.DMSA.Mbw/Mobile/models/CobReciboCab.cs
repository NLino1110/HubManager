using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CobranzasDMSA.Models
{
    //cabeceraCobro
    public class CobReciboCab
    {
        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        public int ID { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        //public int ROWID { get; set; }
        public string CODEMPRESA { get; set; }
        
        //[Key]
        //[PrimaryKey]
        //[NotNull]
        public string IDRECIBO { get; set; }
        public string FECHA { get; set; }
        public string CODUSUARIO { get; set; }
        public string CODCLIENTE { get; set; }
        public string NOMBRECLIENTE { get; set; }
        public string VALORPAGO { get; set; }
        public string DETALLESPAGO { get; set; }
        public string DETALLESDOCU { get; set; }
        public string CODESTADO { get; set; }
        public string TOTALDEUDAACTUAL { get; set; }

        [Ignore]
        public string MANUFACTURER { get; set; }
        [Ignore]
        public string MODEL { get; set; }
        [Ignore]
        public string SERIAL { get; set; }
        [Ignore]
        public string ENVIOAUTOMATICO { get; set; }

        //NOTMAPPED
        //[NotMapped]
        //[IgnoreDataMember]
        //public string? ID { get; set; }
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
