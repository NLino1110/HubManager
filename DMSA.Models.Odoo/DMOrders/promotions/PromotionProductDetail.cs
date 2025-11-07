using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System.ComponentModel.DataAnnotations;

namespace DMSA.Models.Odoo.DMOrders.promotions
{
    [Table("promotion_product_detail")]
    public class PromotionProductDetail : OdooEntity
    {
        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        [JsonProperty("id")]
        public int id { get; set; }

        // -------------------------
        // Many2one
        // -------------------------
        [Ignore]
        [JsonProperty("parent_id")]
        public JToken parent_id { get; set; } // promotion.product

        [JsonIgnore]
        public int _parent_id
        {
            get => GetId(parent_id);
            set => parent_id = SetId(parent_id, value);
        }

        [Ignore]
        [JsonProperty("product_id")]
        public JToken product_id { get; set; } // product.template

        [JsonIgnore]
        public int _product_id
        {
            get => GetId(product_id);
            set => product_id = SetId(product_id, value);
        }

        [JsonIgnore]
        public PromotionProduct product
        {
            //get => GetId(product_id);
            //set => product_id = SetId(product_id, value);
            get => new PromotionProduct();
        }

        [Ignore]
        [JsonProperty("promo_id")]
        public JToken promo_id { get; set; } // promotion.benefit

        [JsonIgnore]
        public int _promo_id
        {
            get => GetId(promo_id);
            set => promo_id = SetId(promo_id, value);
        }

        [Ignore]
        [JsonProperty("bonus_id")]
        public JToken bonus_id { get; set; } // promo.rules

        [JsonIgnore]
        public int _bonus_id
        {
            get => GetId(bonus_id);
            set => bonus_id = SetId(bonus_id, value);
        }

        [Ignore]
        [JsonProperty("product_uom_id")]
        public JToken product_uom_id { get; set; } // uom.uom

        [JsonIgnore]
        public int _product_uom_id
        {
            get => GetId(product_uom_id);
            set => product_uom_id = SetId(product_uom_id, value);
        }

        // Relación a sí mismo (Many2one)
        [Ignore]
        [JsonProperty("product_relation_id")]
        public JToken product_relation_id { get; set; } // promotion.product.detail

        [JsonIgnore]
        public int _product_relation_id
        {
            get => GetId(product_relation_id);
            set => product_relation_id = SetId(product_relation_id, value);
        }

        // -------------------------
        // Campos simples
        // -------------------------
        [JsonProperty("from_file")]
        public bool from_file { get; set; } = false;

        [JsonProperty("qty")]
        public int qty { get; set; }

        [JsonProperty("discount")]
        public int discount { get; set; }

        [JsonProperty("is_fixed")]
        public string is_fixed { get; set; } = "no"; // 'no' | 'si'

        [Column("create_date")]
        public DateTime create_date { get; set; }
        [Column("write_date")]
        public DateTime write_date { get; set; }
    }

}
