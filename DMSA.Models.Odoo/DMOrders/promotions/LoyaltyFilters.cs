using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System.ComponentModel.DataAnnotations;

namespace DMSA.Models.Odoo.DMOrders.promotions
{   

    [Table("loyalty_filters")]
    public class LoyaltyFilters : OdooEntity
    {
        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        [Column("id")]
        [JsonProperty("id")]
        public int id { get; set; }

        // ------------------ Many2one (persistimos el ID auxiliar) ------------------
        [Ignore]
        [JsonProperty("loyalty_id")]
        public JToken loyalty_id { get; set; }

        [JsonIgnore]
        [Column("loyalty_id")]
        public int _loyalty_id
        {
            get => GetId(loyalty_id);
            set => loyalty_id = SetId(loyalty_id, value);
        }

        // ------------------ Flags ------------------
        [Column("marca")]
        [JsonProperty("marca")]
        public bool marca { get; set; } = false;

        [Column("linea")]
        [JsonProperty("linea")]
        public bool linea { get; set; } = false;

        [Column("categoria")]
        [JsonProperty("categoria")]
        public bool categoria { get; set; } = false;

        [Column("subcategoria")]
        [JsonProperty("subcategoria")]
        public bool subcategoria { get; set; } = false;

        [Column("grupor_tipo")]
        [JsonProperty("grupor_tipo")]
        public bool grupor_tipo { get; set; } = false;

        [Column("tipo_marca")]
        [JsonProperty("tipo_marca")]
        public bool tipo_marca { get; set; } = false;

        // ------------------ One2many (JSON-only) ------------------
        [Ignore]
        [JsonProperty("detail_ids")]
        public JToken detail_ids { get; set; }  // loyalty.filters.detail (type='normal')

        [JsonIgnore]
        public LoyaltyFiltersDetail[] _detail_ids
        {
            //get => GetIds(detail_ids);
            //set => detail_ids = SetIds(detail_ids, value);
            get => detail_ids == null ? Array.Empty<LoyaltyFiltersDetail>() : detail_ids.ToObject<LoyaltyFiltersDetail[]>();
        }

        [Ignore]
        [JsonProperty("changes_ids")]
        public JToken changes_ids { get; set; } // loyalty.filters.detail (type='change')

        [JsonIgnore]
        public int[] _changes_ids
        {
            get => GetIds(changes_ids);
            set => changes_ids = SetIds(changes_ids, value);
        }

        // ------------------ Archivos / Campos de texto ------------------
        [Column("product_file")]
        [JsonProperty("product_file")]
        public string product_file { get; set; }   // binario en base64

        [Column("product_file_name")]
        [JsonProperty("product_file_name")]
        public string product_file_name { get; set; }

        [Column("summary")]
        [JsonProperty("summary")]
        public string summary { get; set; }        // HTML

        // ------------------ Estado / flags extra ------------------
        [Column("upload")]
        [JsonProperty("upload")]
        public bool upload { get; set; } = false;

        [Column("state")]
        [JsonProperty("state")]
        public string state { get; set; } = "draft"; // 'draft'|'to_authorize'|'generate_changes'|'request_changes'|'authorized'|'finalized'

        // ------------------ Plantilla generada ------------------
        [Column("template_file")]
        [JsonProperty("template_file")]
        public string template_file { get; set; }  // binario en base64

        [Column("template_file_name")]
        [JsonProperty("template_file_name")]
        public string template_file_name { get; set; }

        [Column("create_date")]
        public DateTime create_date { get; set; }
        [Column("write_date")]
        public DateTime write_date { get; set; }
    }

}
