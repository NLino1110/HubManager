using CobranzasDMSA_Odoo.Models;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.General.Responses
{
    [Obsolete("Debe ser eliminado")]
    public class ApiResponseResUser: ApiResponseOdoo
    {
        //public bool success { get; set; }
        //public string message { get; set; }
        //public int responseCode { get; set; }
        //public string api_key { get; set; }
        //public string fields { get; set; }
        //public string domain { get; set; }
        //public string object_name { get; set; }
        //public Permisssions permisssions { get; set; }
        //public int model_id { get; set; }
        //public Datum[] data { get; set; }
        public res_user[] data { get; set; }
    }

    [Obsolete("Debe ser eliminado")]
    public class Datum
    {
        public string login { get; set; }
        public res_company[] company_id { get; set; }
        public res_partner[] partner_id { get; set; }
        public int id { get; set; }
    }
}
