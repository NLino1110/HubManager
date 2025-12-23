using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace DMSA.Models.Odoo.Native.Inventory
{
    [Table("wms_stock_quant")]
    public class wms_stock_quant : OdooEntity
    {
        [PrimaryKey]
        public int id { get; set; }

        [JsonIgnore]
        public int _product_id
        {
            get => GetId(product_id);
            set => product_id = SetId(product_id, value);
        }

        [JsonIgnore]
        public int _product_tmpl_id
        {
            get => GetId(product_tmpl_id);
            set => product_tmpl_id = SetId(product_tmpl_id, value);
        }

        [JsonIgnore]
        public int _location_id
        {
            get => GetId(location_id);
            set => location_id = SetId(location_id, value);
        }

        [JsonIgnore]
        public int _warehouse_id
        {
            get => GetId(warehouse_id);
            set => warehouse_id = SetId(warehouse_id, value);
        }

        [JsonProperty("cantidad_disponible")]
        public float cantidad_disponible { get; set; }

        [JsonProperty("cantidad_reservada")]
        public float cantidad_reservada { get; set; }

        [JsonProperty("cantidad_total")]
        public float cantidad_total { get; set; }

        [JsonProperty("por_actualizar")]
        public bool por_actualizar { get; set; }

        [JsonProperty("display_name")]
        public string display_name { get; set; }
        
        [JsonProperty("create_date")]
        public DateTime? create_date { get; set; }
        [JsonProperty("write_date")]
        public DateTime? write_date { get; set; }
        
        [Ignore] public JToken product_id { get; set; }
        [Ignore] public JToken product_tmpl_id { get; set; }                
        [Ignore] public JToken location_id { get; set; }
        [Ignore] public JToken warehouse_id { get; set; }
        [Ignore] public JToken lot_id { get; set; }
    }
}
