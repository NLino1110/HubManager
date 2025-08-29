using Models.DMSA.Mbw.Clientes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.General
{

    public class ApiResponseClienteAprobacion
    {
        public bool success { get; set; }
        public bool exito { get; set; }
        public int cantidad_registros { get; set; }
        public bool final { get; set; }
        public ClienteAprobacion[]? data { get; set; }
    }
}
