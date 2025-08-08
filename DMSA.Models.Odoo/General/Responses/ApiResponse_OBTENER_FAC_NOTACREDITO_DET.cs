using CobranzasDMSA_Odoo.Models;
using DMSA.Models.Clientes;
using DMSA.Models.MovilCobranzas.Api;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.General.Responses
{
    //Estructura para respuesta de acción PARAMETROS_CXC
    public class ApiResponse_OBTENER_FAC_NOTACREDITO_DET : ApiResponseOdoo
    {
        //public bool success { get; set; }
        //public bool exito { get; set; }
        //public int cantidad_registros { get; set; }
        //public bool final { get; set; }
        public FacNotaCreditoDet[]? data { get; set; }
    }

    public class ApiResponse_account_move_line : ApiResponseOdoo
    {
        public account_move_line[]? data { get; set; }
    }
}
