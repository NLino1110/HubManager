using CobranzasDMSA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.General.Responses
{    
    public class ApiResponse_OBTENER_CARTERA_CAB
    {
        public bool success { get; set; }
        public bool exito { get; set; }
        public int cantidad_registros { get; set; }
        public bool final { get; set; }
        public CobCarteraCab[]? data { get; set; }
    }
}
