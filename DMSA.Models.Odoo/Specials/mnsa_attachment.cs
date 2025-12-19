using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Json.Converters;
using Newtonsoft.Json;
using SQLite;

namespace DMSA.Models.Odoo.Specials
{
    [Table("mnsa_attachment")]
    public class mnsa_attachment : OdooEntity
    {
        [PrimaryKey]
        [JsonProperty("id")]
        public int id { get; set; }
        public string file_content { get; set; }       
        public string file_name { get; set; }
        public string file_type { get; set; }
        public DateTime? date_data_cutoff { get; set; }

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
