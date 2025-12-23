using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Sales
{
    public class wkf_state_order
    {
        public int id { get; set; }
        public string free_order_state { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        public JToken order_id { get; set; }
        public string note { get; set; }
    }
}
