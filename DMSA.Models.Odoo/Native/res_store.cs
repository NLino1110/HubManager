using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SQLite;

namespace DMSA.Models.Odoo.Native
{
    public class res_store
    {
        [PrimaryKey]
        [Column("id")]
        public int id { get; set; }

        [JsonIgnore]
        [Column("company_id")]
        public int company_id { get; set; }

        [Column("create_uid")]
        public int? create_uid { get; set; }
                
        [Column("write_uid")]
        public int? write_uid { get; set; }

        [Column("name")]
        public string? name { get; set; }

        [Column("identifier_mybussines")]
        public string? identifier_mybussines { get; set; }

        [Column("create_date")]
        public DateTime? create_date { get; set; }

        [Column("write_date")]
        public DateTime? write_date { get; set; }

        [Column("latitude")]
        public string? latitude { get; set; }

        [Column("longitude")]
        public string? longitude { get; set; }

        // Navegación
        //public virtual res_user? CreateUser { get; set; }
        //public virtual res_user? WriteUser { get; set; }
    }
}
