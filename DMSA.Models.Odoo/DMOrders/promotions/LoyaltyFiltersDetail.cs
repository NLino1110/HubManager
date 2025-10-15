using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMOrders.promotions
{
    [Table("loyalty_filters_detail")]
    public class LoyaltyFiltersDetail : OdooEntity
    {
        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        [Column("id")]
        [JsonProperty("id")]
        public int id { get; set; }

        // --- Many2one: parent_id -> loyalty.filters ---
        [Ignore]
        [JsonProperty("parent_id")]
        public JToken parent_id { get; set; }

        [JsonIgnore]
        [Column("parent_id")]
        public int _parent_id
        {
            get => GetId(parent_id);
            set => parent_id = SetId(parent_id, value);
        }

        // --- Campos simples ---
        [Column("filter_id")]
        [JsonProperty("filter_id")]
        public int filter_id { get; set; }

        [Column("filter_name")]
        [JsonProperty("filter_name")]
        public string filter_name { get; set; }

        [Column("discount")]
        [JsonProperty("discount")]
        public int discount { get; set; }

        [Column("exclude")]
        [JsonProperty("exclude")]
        public bool exclude { get; set; } = false;

        // --- Many2one: change_id -> loyalty.filters.detail (self) ---
        [Ignore]
        [JsonProperty("change_id")]
        public JToken change_id { get; set; }

        [JsonIgnore]
        [Column("change_id")]
        public int _change_id
        {
            get => GetId(change_id);
            set => change_id = SetId(change_id, value);
        }

        // --- Fechas (Date en Odoo → DateTime? en C#; compara con .Date al usar) ---
        [Column("start_date")]
        [JsonProperty("start_date")]
        public DateTime? start_date { get; set; }

        [Column("end_date")]
        [JsonProperty("end_date")]
        public DateTime? end_date { get; set; }

        // --- Selection ---
        [Column("type")]
        [JsonProperty("type")]
        public string type { get; set; } = "normal"; // 'normal' | 'change'
    }

}
