using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace DMSA.Models.Odoo.Native
{
    [Table("stock_location")]
    public class stock_location : OdooEntity
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }
        public string complete_name { get; set; }
        public bool active { get; set; }
        public string usage { get; set; }
        public bool comment { get; set; }
        public float posx { get; set; }
        public float posy { get; set; }
        public float posz { get; set; }
        public string parent_path { get; set; }
        public bool scrap_location { get; set; }
        //public bool return_location { get; set; }
        public bool replenish_location { get; set; }
        public int cyclic_inventory_frequency { get; set; }
        public string barcode { get; set; }
        public double net_weight { get; set; }
        public double forecast_weight { get; set; }                
        public string display_name { get; set; }
        
        [Column("create_date")]
        public DateTime? create_date { get; set; }

        [Column("write_date")]
        public DateTime? write_date { get; set; }

        [Ignore]
        public JToken location_id { get; set; }
        [Ignore]
        public JToken child_ids { get; set; }
        [Ignore]
        public JToken child_internal_location_ids { get; set; }
        [Ignore]
        public JToken company_id { get; set; }
        [Ignore]
        public JToken removal_strategy_id { get; set; }
        [Ignore]
        public JToken putaway_rule_ids { get; set; }
        [Ignore]
        public JToken quant_ids { get; set; }
        public string last_inventory_date { get; set; }
        [Ignore]
        public JToken next_inventory_date { get; set; }
        [Ignore]
        public JToken warehouse_view_ids { get; set; }
        [Ignore]
        public JToken warehouse_id { get; set; }
        [Ignore]
        public JToken storage_category_id { get; set; }
        [Ignore]
        public JToken outgoing_move_line_ids { get; set; }
        [Ignore]
        public JToken incoming_move_line_ids { get; set; }
        [Ignore]
        public JToken create_uid { get; set; }        
        [Ignore]
        public JToken write_uid { get; set; }
        
        [Ignore]
        public JToken valuation_in_account_id { get; set; }
        [Ignore]
        public JToken valuation_out_account_id { get; set; }
    }
}
