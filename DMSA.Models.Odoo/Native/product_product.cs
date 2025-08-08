using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace DMSA.Models.Odoo.Native
{
    [Table("product_product")]
    public class product_product: OdooEntity
    {
        [PrimaryKey]
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("default_code")]
        public string default_code { get; set; }

        [JsonProperty("code")]
        public string code { get; set; }

        [JsonProperty("partner_ref")]
        public string partner_ref { get; set; }

        [JsonProperty("active")]
        public bool active { get; set; }

        [Ignore]
        [JsonProperty("product_tmpl_id")]
        public JToken product_tmpl_id { get; set; }

        [JsonIgnore]
        public int _product_tmpl_id
        {
            get => GetId(product_tmpl_id);
            set => product_tmpl_id = SetId(product_tmpl_id, value);
        }

        [JsonProperty("barcode")]
        public string barcode { get; set; }

        [JsonProperty("volume")]
        public float volume { get; set; }

        [JsonProperty("weight")]
        public float weight { get; set; }

        [JsonProperty("image_256")]
        public string image_256 { get; set; }

        [JsonProperty("can_image_1024_be_zoomed")]
        public bool can_image_1024_be_zoomed { get; set; }

        [JsonProperty("display_name")]
        public string display_name { get; set; }

        [JsonProperty("create_date")]
        public DateTime create_date { get; set; }

        [JsonProperty("write_date")]
        public DateTime write_date { get; set; }

        [JsonProperty("qty_available")]
        public float qty_available { get; set; }

        [JsonProperty("virtual_available")]
        public float virtual_available { get; set; }

        [JsonProperty("free_qty")]
        public float free_qty { get; set; }

        [JsonProperty("list_price")]
        public float list_price { get; set; }

        [JsonProperty("base_unit_count")]
        public float base_unit_count { get; set; }

        //[JsonProperty("base_unit_id")]
        //public object base_unit_id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("sequence")]
        public int sequence { get; set; }

        [JsonProperty("detailed_type")]
        public string detailed_type { get; set; }

        [JsonProperty("type")]
        public string type { get; set; }

        [Ignore]
        [JsonProperty("categ_id")]
        public JToken categ_id { get; set; }

        [JsonIgnore]
        public int _categ_id
        {
            get => GetId(categ_id);
            set => categ_id = SetId(categ_id, value);
        }

        [Ignore]
        [JsonProperty("uom_id")]
        public JToken uom_id { get; set; }

        [JsonIgnore]
        public int _uom_id
        {
            get => GetId(uom_id);
            set => uom_id = SetId(uom_id, value);
        }

        [Ignore]
        [JsonProperty("product_brand_id")]
        public JToken product_brand_id { get; set; }

        [JsonIgnore]
        public int _product_brand_id
        {
            get => GetId(product_brand_id);
            set => product_brand_id = SetId(product_brand_id, value);
        }

        [JsonProperty("website_url")]
        public string website_url { get; set; }


        [Ignore]
        [JsonProperty("macro_product_line_id")]
        public JToken macro_product_line_id { get; set; }

        [JsonIgnore]
        public int _macro_product_line_id
        {
            get => GetId(macro_product_line_id);
            set => macro_product_line_id = SetId(macro_product_line_id, value);
        }

        [Ignore]
        [JsonProperty("macro_product_group_brand_id")]
        public JToken macro_product_group_brand_id { get; set; }

        [JsonIgnore]
        public int _macro_product_group_brand_id
        {
            get => GetId(macro_product_group_brand_id);
            set => macro_product_group_brand_id = SetId(macro_product_group_brand_id, value);
        }

        [Ignore]
        [JsonProperty("marco_product_subcategory_id")]
        public JToken marco_product_subcategory_id { get; set; }

        [JsonIgnore]
        public int _marco_product_subcategory_id
        {
            get => GetId(marco_product_subcategory_id);
            set => marco_product_subcategory_id = SetId(marco_product_subcategory_id, value);
        }
    }
}
