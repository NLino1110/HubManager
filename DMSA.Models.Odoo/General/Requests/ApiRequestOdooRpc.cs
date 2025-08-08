using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.General.Requests
{
    public class ApiRequestOdooRpc
    {
        public string jsonrpc { get; set; } = "2.0";
        public string method { get; set; }

        [JsonProperty("params")]
        public ParamsRpc _params { get; set; }
        public int id { get; set; }
        public ContextRpc context { get; set; }
    }

    public class ParamsRpc
    {
        public string service { get; set; }
        public string model { get; set; }
        public string method { get; set; }
        public object[] args { get; set; }
    }

    public class ContextRpc
    {
        public string lang { get; set; }
    }
}
