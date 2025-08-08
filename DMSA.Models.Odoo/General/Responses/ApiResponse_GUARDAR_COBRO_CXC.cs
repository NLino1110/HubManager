using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.General.Responses
{
    /// <summary>
    /// GUARDAR_COBRO_CXC
    /// </summary>    
    public class ApiResponse_GUARDAR_COBRO_CXC
    {
        public bool success { get; set; }
        public bool exito { get; set; }
        public string idrecibo { get; set; }
        public string mensaje { get; set; }
    }
}
