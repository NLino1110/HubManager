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
    public class ApiResponseCheckCn : ApiResponseOdoo
    {
        public check_cn_response[] data { get; set; }
    }


    public class check_cn_response
    {
        public int id { get; set; }
        public string invoice_date { get; set; }
        public string name { get; set; }
        public int product_id { get; set; }
        public string product_code { get; set; }
        public string product_name { get; set; }
        public string display_type { get; set; }
        public float quantity { get; set; }
        public float quantity_already_return { get; set; }
        public float max_allow_return { get; set; }
    }

}
