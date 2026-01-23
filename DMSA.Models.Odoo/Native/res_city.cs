using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace DMSA.Models.Odoo.Native
{
    [Table("res_city")]
    public class res_city : OdooEntity
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string display_name { get; set; }
        public string zip { get; set; }
        [Ignore]
        public JToken state_id { get; set; }
        public bool active { get; set; }

        [JsonProperty("create_date")]
        [Column("create_date")]
        public DateTime? create_date { get; set; }

        [JsonProperty("write_date")]
        [Column("write_date")]
        public DateTime? write_date { get; set; }
    }
}
