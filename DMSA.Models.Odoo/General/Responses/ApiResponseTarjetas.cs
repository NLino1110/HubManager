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
    public class ApiResponseTarjetas : ApiResponseOdoo
    {       
        public st_tarjetas[] data { get; set; }
    }
}
