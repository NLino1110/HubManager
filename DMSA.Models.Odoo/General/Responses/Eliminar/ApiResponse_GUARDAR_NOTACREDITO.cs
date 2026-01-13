using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.General.Responses
{
    [Obsolete]
    /// <summary>
    /// GUARDAR_COBRO_CXC
    /// </summary>    
    public class ApiResponse_GUARDAR_NOTACREDITO
    {
        public bool success { get; set; }
        public bool exito { get; set; }
        public string? numcmprventa { get; set; }
        public string mensaje { get; set; }
    }
}
