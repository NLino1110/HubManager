using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Native
{
    public class product_marca
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
        public string clave_externa { get; set; }

        [JsonProperty("create_date")]
        public DateTime create_date { get; set; }

        [JsonProperty("write_date")]
        public DateTime write_date { get; set; }
    }
}
