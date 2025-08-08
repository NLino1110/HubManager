using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace DMSA.Models.Odoo.Native
{
    [Table("stock_warehouse")]
    public class stock_warehouse : OdooEntity
    {
        [PrimaryKey]
        public int id { get; set; }

        public string name { get; set; }
        public bool active { get; set; }
        public string code { get; set; }
        public string reception_steps { get; set; }
        public string delivery_steps { get; set; }
        public int sequence { get; set; }
        public string display_name { get; set; }
        public DateTime create_date { get; set; }
        public DateTime write_date { get; set; }        
        public bool manufacture_to_resupply { get; set; }
        public bool buy_to_resupply { get; set; }


        [JsonIgnore]
        public int _company_id
        {
            get => GetId(company_id);
            set => company_id = SetId(company_id, value);
        }

        [Ignore]
        public JToken company_id { get; set; }
        [Ignore]
        public JToken partner_id { get; set; }
        [Ignore]
        public JToken view_location_id { get; set; }
        [Ignore]
        public JToken lot_stock_id { get; set; }
        [Ignore]
        public JToken route_ids { get; set; }
        [Ignore]
        public JToken wh_input_stock_loc_id { get; set; }
        [Ignore]
        public JToken wh_qc_stock_loc_id { get; set; }
        [Ignore]
        public JToken wh_output_stock_loc_id { get; set; }
        [Ignore]
        public JToken wh_pack_stock_loc_id { get; set; }
        [Ignore]
        public JToken mto_pull_id { get; set; }
        [Ignore]
        public JToken pick_type_id { get; set; }
        [Ignore]
        public JToken pack_type_id { get; set; }
        [Ignore]
        public JToken out_type_id { get; set; }
        [Ignore]
        public JToken in_type_id { get; set; }
        [Ignore]
        public JToken int_type_id { get; set; }
        [Ignore]
        public JToken return_type_id { get; set; }
        [Ignore]
        public JToken crossdock_route_id { get; set; }
        [Ignore]
        public JToken reception_route_id { get; set; }
        [Ignore]
        public JToken delivery_route_id { get; set; }
        [Ignore]
        public JToken resupply_wh_ids { get; set; }
        [Ignore]
        public JToken resupply_route_ids { get; set; }
        [Ignore]
        public JToken create_uid { get; set; }
        [Ignore]
        public JToken write_uid { get; set; }
        [Ignore]
        public JToken manufacture_pull_id { get; set; }
        [Ignore]
        public JToken manufacture_mto_pull_id { get; set; }
        [Ignore]
        public JToken pbm_mto_pull_id { get; set; }
        [Ignore]
        public JToken sam_rule_id { get; set; }
        [Ignore]
        public JToken manu_type_id { get; set; }
        [Ignore]
        public JToken pbm_type_id { get; set; }
        [Ignore]
        public JToken sam_type_id { get; set; }
        [Ignore]
        public JToken pbm_route_id { get; set; }
        [Ignore]
        public JToken pbm_loc_id { get; set; }
        [Ignore]
        public JToken sam_loc_id { get; set; }
        [Ignore]
        public JToken pos_type_id { get; set; }
        [Ignore]
        public JToken buy_pull_id { get; set; }
    }
}
