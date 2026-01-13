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
    //public class ApiResponse_OBTENER_FAC_NOTACREDITO_CAB
    //{
    //    public bool success { get; set; }
    //    public bool exito { get; set; }
    //    public int cantidad_registros { get; set; }
    //    public bool final { get; set; }
    //    public FacNotaCreditoCab[]? data { get; set; }
    //}
    [Obsolete]
    public class ApiResponse_OBTENER_FAC_NOTACREDITO_CAB: ApiResponseOdoo
    {
        public FacNotaCreditoCab[]? data { get; set; }
        //public FacNotaCreditoCab[]? data { get; set; }
        //TODO: Crear constructor con opciones de datos por defecto como por ejemplo 
        // error de conexión
    }

    [Obsolete("Debe ser eliminado")]
    public class ApiResponse_account_move : ApiResponseOdoo
    {
        public account_move[]? data { get; set; }
        //public FacNotaCreditoCab[]? data { get; set; }
        //TODO: Crear constructor con opciones de datos por defecto como por ejemplo 
        // error de conexión
    }
}
