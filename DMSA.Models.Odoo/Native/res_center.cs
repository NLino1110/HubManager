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
    public class res_center
    {
        [PrimaryKey]
        [Column("id")]
        public int id { get; set; }

        [JsonIgnore]
        [Column("company_id")]
        public int company_id { get; set; }

        [Column("name")]
        public string? name { get; set; }

        [Column("type_center")]
        public string? type_center { get; set; }

        [Column("create_date")]
        public DateTime? create_date { get; set; }

        [Column("write_date")]
        public DateTime? write_date { get; set; }
    }
}
