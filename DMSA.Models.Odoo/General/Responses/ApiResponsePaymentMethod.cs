using CobranzasDMSA_Odoo.Models;
using DMSA.Models.MovilCobranzas.Api;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.General.Responses
{
    [Obsolete("Debe ser eliminado")]
    public class ApiResponsePaymentMethod: ApiResponseOdoo
    {       
        public st_formaspago[] data { get; set; }
    }

    //public class Datum_pm
    //{
    //    public int id { get; set; }        
    //    public string? name { get; set; }
    //}
}
