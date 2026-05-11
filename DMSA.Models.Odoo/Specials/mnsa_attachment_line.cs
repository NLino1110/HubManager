using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace DMSA.Models.Odoo.Specials
{
    [Table("mnsa_attachment_line")]
    public class mnsa_attachment_line : OdooEntity
    {
        [PrimaryKey]
        [JsonProperty("id")]
        public int id { get; set; }
        public string? name { get; set; }
        public string url { get; set; }
        public string? file_name { get; set; }        
        public string? file_type { get; set; }
        public int total_file_size_expected { get; set; }
        public bool? success_upload { get; set; }

        public byte[]? file_bytes { get; set; }
        public int package_id { get; set; }

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
