using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using SQLite;
using System.ComponentModel.DataAnnotations;

namespace DMSA.Models.Odoo.DMOrders.promotions
{
    [Table("pos_tarjetas_canal")]
    public class PosTarjetasCanal : OdooEntity
    {
        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("active")]
        public bool active { get; set; } = true;

        [JsonProperty("name")]
        public string name { get; set; }

        // Nombre técnico
        [JsonProperty("name_tech")]
        public string name_tech { get; set; }

        [JsonProperty("create_date")]
        [Column("create_date")]
        public DateTime? create_date { get; set; }


        [JsonProperty("write_date")]
        [Column("write_date")]
        public DateTime? write_date { get; set; }
    }

}
