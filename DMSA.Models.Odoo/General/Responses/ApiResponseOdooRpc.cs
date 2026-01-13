using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.General.Responses
{
    public class ApiResponseOdooRpc
    {
        public string jsonrpc { get; set; }
        public int id { get; set; }
        public int result { get; set; }
        public Error? error { get; set; }
    }
}
