using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace DMSA.Models.Odoo.Native
{
    [Table("stock_quant")]
    public class stock_quant : OdooEntity
    {
        [PrimaryKey]
        public int id { get; set; }

        [JsonIgnore]
        public int _product_id
        {
            get => GetId(product_id);
            set => product_id = SetId(product_id, value);
        }

        //public string priority { get; set; }
        public double quantity { get; set; }
        public double reserved_quantity { get; set; }
        public double available_quantity { get; set; }
        public DateTime in_date { get; set; }
        public string tracking { get; set; }
        public bool on_hand { get; set; }
        public double inventory_quantity { get; set; }
        public double inventory_quantity_auto_apply { get; set; }
        public double inventory_diff_quantity { get; set; }
        public DateTime inventory_date { get; set; }
        [JsonConverter(typeof(OdooNullableDateTimeConverter))]
        public DateTime? last_count_date { get; set; }
        public bool inventory_quantity_set { get; set; }
        public bool is_outdated { get; set; }        
        public string display_name { get; set; }
        public DateTime create_date { get; set; }
        public DateTime write_date { get; set; }
        public bool use_expiration_date { get; set; }
        public double value { get; set; }
        public string cost_method { get; set; }
        //public string dummy_id { get; set; }

        [Ignore] public JToken product_id { get; set; }
        [Ignore] public JToken product_tmpl_id { get; set; }
        [Ignore] public JToken product_uom_id { get; set; }
        [Ignore] public JToken company_id { get; set; }
        [Ignore] public JToken location_id { get; set; }
        [Ignore] public JToken warehouse_id { get; set; }
        [Ignore] public JToken storage_category_id { get; set; }
        [Ignore] public JToken lot_id { get; set; }
        [Ignore] public JToken sn_duplicated { get; set; }
        [Ignore] public JToken package_id { get; set; }
        [Ignore] public JToken owner_id { get; set; }
        [Ignore] public JToken product_categ_id { get; set; }
        [Ignore] public JToken user_id { get; set; }
        [Ignore] public JToken removal_date { get; set; }
        [Ignore] public JToken accounting_date { get; set; }
        [Ignore] public JToken currency_id { get; set; }
        [Ignore] public JToken create_uid { get; set; }
        [Ignore] public JToken write_uid { get; set; }
    }
}
