using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System.ComponentModel.DataAnnotations;

namespace DMSA.Models.Odoo.DMOrders.promotions
{
    // Tabla equivalente a 'promo.centers' en Odoo
    [Table("promo_centers")]
    public class PromoCenters : OdooEntity
    {
        [Key]
        [PrimaryKey]
        //[AutoIncrement]
        [NotNull]
        [JsonProperty("id")]
        public int id { get; set; }

        // --- Many2one: promo_id -> promotion.benefit ---
        [Ignore]
        [JsonProperty("promo_id")]
        public JToken promo_id { get; set; }

        [JsonIgnore]
        public int _promo_id
        {
            get => GetId(promo_id);
            set => promo_id = SetId(promo_id, value);
        }

        // --- Many2many: res_center_id -> res.center ---
        [Ignore]
        [JsonProperty("res_center_id")]
        public JToken res_center_id { get; set; }

        [Ignore]
        [JsonIgnore]
        public int[] _res_center_id
        {
            get => GetIds(res_center_id);
            set => res_center_id = SetIds(res_center_id, value);
        }

        [JsonIgnore]
        [Column("res_center_id_json")]
        public string res_center_id_json
        {
            get => SetIdsJson(res_center_id);            
            set { }
        }

        //[Ignore]
        //[JsonProperty("levels_ids")]
        //public JToken levels_ids { get; set; }

        //[JsonIgnore]
        //public string levels_ids_json
        //{
        //    get => SetIdsJson(levels_ids);
        //    set { }
        //}

        [Ignore]
        [JsonProperty("levels_ids")]
        public JToken levels_ids
        {
            get => string.IsNullOrEmpty(levels_ids_json)
                ? null
                : JToken.Parse(levels_ids_json);

            set => levels_ids_json = value?.ToString(Newtonsoft.Json.Formatting.None);
        }

        [JsonIgnore]
        [Column("levels_ids_json")]
        public string levels_ids_json { get; set; }

        // --- Campos simples ---
        [JsonProperty("times_inv")]
        public int times_inv { get; set; }

        [JsonProperty("bank_terms_apply")]
        public bool bank_terms_apply { get; set; } = false;

        [JsonProperty("create_date")]
        [Column("create_date")]
        public DateTime? create_date { get; set; }


        [JsonProperty("write_date")]
        [Column("write_date")]
        public DateTime? write_date { get; set; }
    }
}
