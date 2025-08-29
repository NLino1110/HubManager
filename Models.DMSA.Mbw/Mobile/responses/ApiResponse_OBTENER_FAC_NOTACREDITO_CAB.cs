using CobranzasDMSA.Models;
using DMSA.Models.MovilCobranzas.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.General.Responses
{
    //Estructura para respuesta de acción PARAMETROS_CXC
    public class ApiResponse_OBTENER_FAC_NOTACREDITO_CAB
    {
        public bool success { get; set; }
        public bool exito { get; set; }
        public int cantidad_registros { get; set; }
        public bool final { get; set; }
        public FacNotaCreditoCab[]? data { get; set; }
    }
}
