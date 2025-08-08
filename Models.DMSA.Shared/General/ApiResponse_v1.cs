using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DMSA.Shared.General
{

    public class ApiResponse_v1
    {
        public bool success { get; set; }
        public bool exito { get; set; }
        public int cantidad_registros { get; set; }
        public bool final { get; set; }
        public dynamic? data { get; set; }
        public string? mensaje { get; set; } = "Proceso realizado exitosamente";
    }
}
