using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace DMSA.Models.Odoo.Specials
{
    [Table("ir_attachment")]
    public class ir_attachment : OdooEntity
    {
        [PrimaryKey]
        [JsonProperty("id")]
        public int id { get; set; }
        public string name { get; set; }
        public string display_name { get; set; }
        public string description { get; set; }        
        public string type { get; set; }
        public int file_size { get; set; }
        public string url { get; set; }
        public string local_url { get; set; }
        public string checksum { get; set; }
        public string datas { get; set; }
        public string mimetype { get; set; }
        public string res_field { get; set; }
        public string res_model { get; set; }
        public int res_id { get; set; }

        [Column("create_date")]
        [JsonProperty("create_date")]
        [JsonConverter(typeof(OdooNullableDateTimeConverter))]
        public DateTime create_date { get; set; }

        [Column("write_date")]
        [JsonProperty("write_date")]
        [JsonConverter(typeof(OdooNullableDateTimeConverter))]
        public DateTime write_date { get; set; }
    }
}
