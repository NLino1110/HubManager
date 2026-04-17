using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.General.Responses
{
    public class ApiResponse_VALIDASINCRONIZACION
    {
        public bool success { get; set; }
        public string message { get; set; }
        public int responseCode { get; set; }
        public Datum[] data { get; set; }
    }

    public class Datum
    {
        public DateTime datetime { get; set; }
        //public string companies { get; set; }
        public res_company[]? companies { get; set; }
        //public string boo { get; set; }
    }

}
