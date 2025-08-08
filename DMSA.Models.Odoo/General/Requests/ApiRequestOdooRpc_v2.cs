using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.General.Requests
{
    public class ApiRequestOdooRpc_v2
    {
        public int id { get; set; } = 1;
        public string jsonrpc { get; set; } = "2.0";
        public string method { get; set; } = "call";
        
        [JsonProperty("params")]
        public Params_v2 _params { get; set; }
    }

    public class Params_v2
    {
        public string model { get; set; }
        public string method { get; set; }
        public object[] args { get; set; }
        public object kwargs { get; set; }
        //public Kwargs_v2 kwargs { get; set; }
        //public Context context { get; set; }

        public string db { get; set; }
        public string login { get; set; }
        public string password { get; set; }
    }

    public class Kwargs_v2
    {
        
    }
}
