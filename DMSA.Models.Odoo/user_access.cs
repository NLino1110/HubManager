using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Models
{
    [Obsolete("Debe ser eliminado")]
    public class user_access_delete
    {
        //[PrimaryKey]
        //[NotNull]
        [PrimaryKey]
        public int uid { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string pwd { get; set; }
        public string companies { get; set; }
        //public res_company[] COMPANIES { get; set; }

        //Campos con nombres confusos
        [Obsolete]
        public string ULTIMAACTUALIZACION { get; set; }
        [Obsolete]
        public string FECHAACTNC { get; set; }
        [Obsolete]
        public string FECHAACTUAL { get; set; }
        //Campos con nombres confusos --fin

        public DateTime log_fec_acceso { get; set; }
        public DateTime log_fec_sincro { get; set; }
        public DateTime log_fec_sincro_nc { get; set; }

        public string api_key { get; set; }
        public string token_type { get; set; }
        public string access_token { get; set; }
        public string databasename { get; set; }
    }
}
