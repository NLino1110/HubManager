using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System.ComponentModel.DataAnnotations;

namespace DMSA.Models.Odoo.Accounting
{
    [Table("type_parent_nc")]
    public class TypeParentNc : OdooEntity
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

        [JsonProperty("code")]
        [Column("code")]
        public string? code { get; set; }

        [JsonProperty("motivo_val_dev_nc")]
        [Column("motivo_val_dev_nc")]
        public string? motivo_val_dev_nc { get; set; }

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
