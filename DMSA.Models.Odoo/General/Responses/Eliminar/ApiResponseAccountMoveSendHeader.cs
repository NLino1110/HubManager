using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.General.Responses
{
    [Obsolete("Debe ser eliminado")]
    public class ApiResponseAccountMoveSendHeader : ApiResponseOdoo
    {
        public AccountMoveSendHeader[] data { get; set; }
    }
}
