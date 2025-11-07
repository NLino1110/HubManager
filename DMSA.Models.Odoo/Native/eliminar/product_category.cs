using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Native
{
    [Obsolete]
    public class product_category : OdooEntity
    {
        [PrimaryKey]
        public int id { get; set; }
        
        [Ignore]
        [JsonProperty("parent_id")]
        public JToken parent_id { get; set; }

        [JsonIgnore]
        public int _parent_id
        {
            get => GetId(parent_id);
            set => parent_id = SetId(parent_id, value);
        }

        public string name { get; set; }
        public string complete_name { get; set; }
        public string parent_path { get; set; }
        public int product_count { get; set; }        
        public string packaging_reserve_method { get; set; }
        public string property_valuation { get; set; }
        public string property_cost_method { get; set; }        

        [JsonProperty("create_date")]
        public DateTime create_date { get; set; }

        [JsonProperty("write_date")]
        public DateTime write_date { get; set; }
    }
}
