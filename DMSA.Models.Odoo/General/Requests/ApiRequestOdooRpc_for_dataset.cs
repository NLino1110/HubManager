using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.General.Requests
{  

    public class ApiRequestOdooRpc_for_dataset
    {
        public int id { get; set; } = 1;
        public string jsonrpc { get; set; } = "2.0";
        public string method { get; set; } = "call";
        
        [JsonProperty("params")]
        public Params _params { get; set; }
    }

    public class Params
    {
        public string model { get; set; }
        public string method { get; set; }
        public object[] args { get; set; }
        public Kwargs kwargs { get; set; }
        //public Context context { get; set; }
    }

    public class Kwargs
    {
        [JsonIgnore]
        public string[][] domain { get; set; }
        public Context context { get; set; }
    }

    public class Context
    {
        public string lang { get; set; }
        public string tz { get; set; }
        public int uid { get; set; }
        public int[] allowed_company_ids { get; set; }
    }

}
