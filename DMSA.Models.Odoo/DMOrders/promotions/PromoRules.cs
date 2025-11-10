using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System.ComponentModel.DataAnnotations;

namespace DMSA.Models.Odoo.DMOrders.promotions
{
    [Table("promo_rules")]
    public class PromoRules : OdooEntity
    {
        [Key]
        [PrimaryKey]
        //[AutoIncrement]
        [NotNull]
        [JsonProperty("id")]
        public int id { get; set; }

        // ----------------------------------------------------------------------
        // Many2one
        // ----------------------------------------------------------------------
        [Ignore]
        [JsonProperty("promo_id")]
        public JToken promo_id { get; set; }

        [JsonIgnore]
        public int _promo_id
        {
            get => GetId(promo_id);
            set => promo_id = SetId(promo_id, value);
        }

        // (related en Odoo) lo modelamos explícito para simplificar API
        [Ignore]
        [JsonProperty("promotion_type_id")]
        public JToken promotion_type_id { get; set; }

        [JsonIgnore]
        public int _promotion_type_id
        {
            get => GetId(promotion_type_id);
            set => promotion_type_id = SetId(promotion_type_id, value);
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

        [Ignore]
        [JsonProperty("product_uom_id")]
        public JToken product_uom_id { get; set; } // uom.uom

        [JsonIgnore]
        public int _product_uom_id
        {
            get => GetId(product_uom_id);
            set => product_uom_id = SetId(product_uom_id, value);
        }

        [Ignore]
        [JsonProperty("selection_type_id")]
        public JToken selection_type_id { get; set; }

        [JsonIgnore]
        public int _selection_type_id
        {
            get => GetId(selection_type_id);
            set => selection_type_id = SetId(selection_type_id, value);
        }

        [Ignore]
        [JsonProperty("payment_method_id")]
        public JToken payment_method_id { get; set; } // pos.payment.method

        [JsonIgnore]
        public int _payment_method_id
        {
            get => GetId(payment_method_id);
            set => payment_method_id = SetId(payment_method_id, value);
        }

        [Ignore]
        [JsonProperty("raffle_template_id")]
        public JToken raffle_template_id { get; set; }

        [JsonIgnore]
        public int _raffle_template_id
        {
            get => GetId(raffle_template_id);
            set => raffle_template_id = SetId(raffle_template_id, value);
        }

        [Ignore]
        [JsonProperty("change_id")]
        public JToken change_id { get; set; } // self Many2one

        [JsonIgnore]
        public int _change_id
        {
            get => GetId(change_id);
            set => change_id = SetId(change_id, value);
        }

        // ----------------------------------------------------------------------
        // One2many
        // ----------------------------------------------------------------------
        [Ignore]
        [JsonProperty("product_promotion_ids")]
        public JToken product_promotion_ids { get; set; } // promotion.product (bonus_id)

        [Ignore]
        [JsonIgnore]
        public int[] _product_promotion_ids
        {
            get => GetIds(product_promotion_ids);
            set => product_promotion_ids = SetIds(product_promotion_ids, value);
        }

        // ----------------------------------------------------------------------
        // Many2many (arrays de IDs)
        // ----------------------------------------------------------------------
        [Ignore]
        [JsonProperty("general_marca_id")]
        public JToken general_marca_id { get; set; }

        [Ignore]
        [JsonIgnore]
        public int[] _general_marca_id
        {
            get => GetIds(general_marca_id);
            set => general_marca_id = SetIds(general_marca_id, value);
        }

        [Ignore]
        [JsonProperty("general_linea_id")]
        public JToken general_linea_id { get; set; }

        [Ignore]
        [JsonIgnore]
        public int[] _general_linea_id
        {
            get => GetIds(general_linea_id);
            set => general_linea_id = SetIds(general_linea_id, value);
        }

        [Ignore]
        [JsonProperty("general_categoria_id")]
        public JToken general_categoria_id { get; set; }

        [Ignore]
        [JsonIgnore]
        public int[] _general_categoria_id
        {
            get => GetIds(general_categoria_id);
            set => general_categoria_id = SetIds(general_categoria_id, value);
        }

        [Ignore]
        [JsonProperty("general_subcategoria_id")]
        public JToken general_subcategoria_id { get; set; }

        [Ignore]
        [JsonIgnore]
        public int[] _general_subcategoria_id
        {
            get => GetIds(general_subcategoria_id);
            set => general_subcategoria_id = SetIds(general_subcategoria_id, value);
        }

        [Ignore]
        [JsonProperty("general_grupor_tipo_id")]
        public JToken general_grupor_tipo_id { get; set; }

        [Ignore]
        [JsonIgnore]
        public int[] _general_grupor_tipo_id
        {
            get => GetIds(general_grupor_tipo_id);
            set => general_grupor_tipo_id = SetIds(general_grupor_tipo_id, value);
        }

        [Column("general_grupor_tipo_id")]
        public string general_grupor_tipo_id_json
        {
            get => general_grupor_tipo_id?.ToString(Formatting.None);
            set => general_grupor_tipo_id = string.IsNullOrEmpty(value)
                ? null
                : JToken.Parse(value);
        }

        [Ignore]
        [JsonProperty("general_tipo_marca_id")]
        public JToken general_tipo_marca_id { get; set; }

        [Ignore]
        [JsonIgnore]
        public int[] _general_tipo_marca_id
        {
            get => GetIds(general_tipo_marca_id);
            set => general_tipo_marca_id = SetIds(general_tipo_marca_id, value);
        }

        // ----------------------------------------------------------------------
        // Campos simples
        // ----------------------------------------------------------------------
        [JsonProperty("variable")]
        public string variable { get; set; } // 'qty_product_unts' | 'total_product_amount' | 'total_order'

        [JsonProperty("operator")]
        public string operator_ { get; set; } // '<' '>' '<=' '>=' '=' '<>' (guardamos la key: 'less_than', etc.)

        [JsonProperty("value")]
        public int value { get; set; }

        [JsonProperty("minimum_value")]
        public int minimum_value { get; set; }

        [JsonProperty("maximum_value")]
        public int maximum_value { get; set; }

        [JsonProperty("product_promotion")]
        public string product_promotion { get; set; } = "no"; // 'no' | 'si'

        [JsonProperty("code")]
        public string code { get; set; }

        [JsonProperty("qty")]
        public int qty { get; set; }

        [JsonProperty("is_fixed")]
        public string is_fixed { get; set; } = "no"; // 'no' | 'si'

        [JsonProperty("discount")]
        public int discount { get; set; }

        [JsonProperty("discount_base")]
        public int discount_base { get; set; }

        [JsonProperty("count_products")]
        public int count_products { get; set; }

        
        [JsonProperty("start_date")]
        [JsonConverter(typeof(OdooNullableDateTimeConverter))]
        public DateTime? start_date { get; set; }

        [JsonProperty("end_date")]
        [JsonConverter(typeof(OdooNullableDateTimeConverter))]
        public DateTime? end_date { get; set; }

        [JsonProperty("unlimited_time")]
        public bool unlimited_time { get; set; } = false;

        [JsonProperty("state")]
        public bool state { get; set; } = true;

        [JsonProperty("type")]
        public string type { get; set; } = "normal"; // 'normal' | 'change'

        [Column("create_date")]
        public DateTime create_date { get; set; }
        [Column("write_date")]
        public DateTime write_date { get; set; }
    }

}
