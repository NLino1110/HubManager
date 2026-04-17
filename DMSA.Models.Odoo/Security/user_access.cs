using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Security
{
    public class user_access
    {
        //[PrimaryKey]
        //[NotNull]
        [PrimaryKey]
        public int uid { get; set; }
        public int partner_id { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string pwd { get; set; }
        public string companies { get; set; }
        //public res_company[] COMPANIES { get; set; }                

        public DateTime log_fec_acceso { get; set; }
        public DateTime log_fec_sincro { get; set; }
        public DateTime log_fec_sincro_nc { get; set; }

        public string api_key { get; set; }
        public string token_type { get; set; }
        public string access_token { get; set; }
        public string databasename { get; set; }
    }
}
