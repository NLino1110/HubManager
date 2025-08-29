using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA.Models
{
    public class CobCierre
    {
        [Key]
        [PrimaryKey]
        [NotNull]
        public string IDCIERRE { get; set; }
        public string CODEMPRESA { get; set; }        
        public int BANCO { get; set; }
        public string NUMDEPOSITO { get; set; }
        public string VALOR { get; set; }
        public string DETALLECIERRE { get; set; }
    }
}
