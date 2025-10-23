using DMSA.Models.Clientes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Clientes.Aprobaciones.Responses
{
    [Obsolete]
    public class ApiResponse_v1
    {
        public bool success { get; set; }
        public dynamic? data { get; set; }
        public string? mensaje { get; set; }
    }
}
