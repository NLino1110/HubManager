using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System.ComponentModel.DataAnnotations;

namespace DMSA.Models.Odoo.DMOrders.promotions
{
    [Table("selection_type")]
    public class PromotionSelectionType : OdooEntity
    {
        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("active")]
        public bool active { get; set; } = true;

        // Many2many -> promotion.type
        [Ignore]
        [JsonProperty("promotion_type_ids")]
        public JToken promotion_type_ids { get; set; }

        [JsonIgnore]
        public int[] _promotion_type_ids
        {
            get => GetIds(promotion_type_ids);
            set => promotion_type_ids = SetIds(promotion_type_ids, value);
        }

        [JsonProperty("create_date")]
        [Column("create_date")]
        public DateTime? create_date { get; set; }


        [JsonProperty("write_date")]
        [Column("write_date")]
        public DateTime? write_date { get; set; }
    }

}
