using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DMSA.Shared.General.v3
{

    public class ApiResponse_save
    {
        public bool success { get; set; } = true;
        public dynamic? data { get; set; }
        public string? mensaje { get; set; } = "Proceso realizado exitosamente";
    }
}
