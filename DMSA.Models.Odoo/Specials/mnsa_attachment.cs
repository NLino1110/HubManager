using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace DMSA.Models.Odoo.Specials
{
    [Table("mnsa_attachment")]
    public class mnsa_attachment : OdooEntity
    {
        [PrimaryKey]
        [AutoIncrement]
        [JsonProperty("id")]
        public int id { get; set; }
        public string server { get; set; }
        public string database_name { get; set; }
        public string file_name { get; set; }
        public string file_type { get; set; }
        public DateTime? date_data_cutoff { get; set; }
        public double total_file_size { get; set; }

        [Ignore]
        public JToken mobile_app_id { get; set; }
     
        [JsonIgnore]
        public int _mobile_app_id
        {
            get => GetId(mobile_app_id);
            set => mobile_app_id = SetId(mobile_app_id, value);
        }

        [Ignore]
        public JToken attachment_ids { get; set; }

        [Ignore]
        [JsonIgnore]
        public int[] _attachment_ids
        {
            get => GetIds(attachment_ids);
            set => attachment_ids = SetIds(attachment_ids, value);
        }

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
