using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA.Models
{
    public class CobUsuarios
    {
        [PrimaryKey]
        [NotNull]
        public string CODUSUARIO { get; set; }
        public string NOMBRE { get; set; }
        public string CLAVE { get; set; }
        public string CLIENTESTODOS { get; set; }
        
        //Campos con nombres confusos
        public string ULTIMAACTUALIZACION { get; set; }
        public string FECHAACTNC { get; set; }
        public string FECHAACTUAL { get; set; }
        //Campos con nombres confusos --fin

        public DateTime log_fec_acceso { get; set; }
        public DateTime log_fec_sincro { get; set; }
        public DateTime log_fec_sincro_nc { get; set; }
    }
}
