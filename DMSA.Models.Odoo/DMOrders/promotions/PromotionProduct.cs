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
    [Table("promotion_product")]
    public class PromotionProduct : OdooEntity
    {
        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        [JsonProperty("id")]
        public int id { get; set; }

        // -------------------------
        // Básicos
        // -------------------------
        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("qty")]
        public int qty { get; set; }

        [JsonProperty("discount")]
        public int discount { get; set; } = 100;

        [JsonProperty("available_discount")]
        public bool available_discount { get; set; } = false;

        [JsonProperty("is_fixed")]
        public string is_fixed { get; set; } = "no"; // 'no' | 'si'

        // Archivo binario: en JSON suele viajar como base64 (string)
        [JsonProperty("product_file")]
        public string product_file { get; set; }

        [JsonProperty("product_file_name")]
        public string product_file_name { get; set; }

        [JsonProperty("product_id_count")]
        public int product_id_count { get; set; }

        [JsonProperty("show_detail")]
        public bool show_detail { get; set; } = false;

        [JsonProperty("imported_from_file")]
        public bool imported_from_file { get; set; } = false;

        [JsonProperty("all_products")]
        public bool all_products { get; set; } = false;

        [JsonProperty("summary")]
        public string summary { get; set; }

        [JsonProperty("view_type_name")]
        public string view_type_name { get; set; } // selection -> string

        [JsonProperty("state")]
        public string state { get; set; } = "draft"; // 'draft'|'nxn'|'to_authorize'|'authorized'|'finalized'

        [JsonProperty("last_run_signature")]
        public string last_run_signature { get; set; }

        [JsonProperty("last_run_at")]
        public DateTime? last_run_at { get; set; }

        // -------------------------
        // Many2one
        // -------------------------
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

        // -------------------------
        // Many2many
        // -------------------------
        [Ignore]
        [JsonProperty("general_marca_id")]
        public JToken general_marca_id { get; set; }

        [JsonIgnore]
        public int[] _general_marca_id
        {
            get => GetIds(general_marca_id);
            set => general_marca_id = SetIds(general_marca_id, value);
        }

        [Ignore]
        [JsonProperty("general_linea_id")]
        public JToken general_linea_id { get; set; }

        [JsonIgnore]
        public int[] _general_linea_id
        {
            get => GetIds(general_linea_id);
            set => general_linea_id = SetIds(general_linea_id, value);
        }

        [Ignore]
        [JsonProperty("general_categoria_id")]
        public JToken general_categoria_id { get; set; }

        [JsonIgnore]
        public int[] _general_categoria_id
        {
            get => GetIds(general_categoria_id);
            set => general_categoria_id = SetIds(general_categoria_id, value);
        }

        [Ignore]
        [JsonProperty("general_subcategoria_id")]
        public JToken general_subcategoria_id { get; set; }

        [JsonIgnore]
        public int[] _general_subcategoria_id
        {
            get => GetIds(general_subcategoria_id);
            set => general_subcategoria_id = SetIds(general_subcategoria_id, value);
        }

        [Ignore]
        [JsonProperty("general_grupor_tipo_id")]
        public JToken general_grupor_tipo_id { get; set; }

        [JsonIgnore]
        public int[] _general_grupor_tipo_id
        {
            get => GetIds(general_grupor_tipo_id);
            set => general_grupor_tipo_id = SetIds(general_grupor_tipo_id, value);
        }

        [Ignore]
        [JsonProperty("general_tipo_marca_id")]
        public JToken general_tipo_marca_id { get; set; }

        [JsonIgnore]
        public int[] _general_tipo_marca_id
        {
            get => GetIds(general_tipo_marca_id);
            set => general_tipo_marca_id = SetIds(general_tipo_marca_id, value);
        }

        [Ignore]
        [JsonProperty("general_product_id")]
        public JToken general_product_id { get; set; } // product.template m2m

        [JsonIgnore]
        public int[] _general_product_id
        {
            get => GetIds(general_product_id);
            set => general_product_id = SetIds(general_product_id, value);
        }

        // -------------------------
        // One2many
        // -------------------------
        [Ignore]
        [JsonProperty("detail_ids")]
        public JToken detail_ids { get; set; } // promotion.product.detail (parent_id)

        [JsonIgnore]
        public int[] _detail_ids
        {
            get => GetIds(detail_ids);
            set => detail_ids = SetIds(detail_ids, value);
        }
    }

}
