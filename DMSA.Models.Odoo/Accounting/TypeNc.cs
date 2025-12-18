using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System.ComponentModel.DataAnnotations;

namespace DMSA.Models.Odoo.Accounting
{
    [Table("type_nc")]
    public class TypeNc : OdooEntity
    {
        [Key]
        [PrimaryKey]        
        [NotNull]
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("name")]
        [Column("name")]
        public string? name { get; set; }

        [JsonProperty("display_name")]
        [Column("display_name")]
        public string? display_name { get; set; }

        [JsonProperty("nc_type")]
        [Column("nc_type")]
        public string? nc_type { get; set; }

        [Ignore]
        [JsonProperty("parent_id")]
        public JToken parent_id { get; set; }

        [JsonIgnore]
        public int _parent_id
        {
            get => GetId(parent_id);
            set => parent_id = SetId(parent_id, value);
        }

        [Ignore]
        [JsonProperty("account_id")]
        public JToken account_id { get; set; }

        [JsonIgnore]
        public int _account_id
        {
            get => GetId(account_id);
            set => account_id = SetId(account_id, value);
        }

        [JsonProperty("active")]
        [Column("active")]
        public bool active { get; set; } = true;

        [JsonProperty("create_date")]
        [Column("create_date")]
        public DateTime? create_date { get; set; }

        [JsonProperty("write_date")]
        [Column("write_date")]
        public DateTime? write_date { get; set; }
    }
}
