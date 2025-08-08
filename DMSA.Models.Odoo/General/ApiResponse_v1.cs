using DMSA.Models.Clientes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.General
{
    public class ApiResponse_v1
    {
        public string? success { get; set; }
        public string? exito { get; set; }
        public int cantidad_registros { get; set; }
        public string? final { get; set; }
        public dynamic? data { get; set; }
    }
}
