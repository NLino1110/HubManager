using DMSA.Models.Clientes;
using DMSA.Models.MovilCobranzas.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.General
{
    //Estructura para respuesta de acción PARAMETROS_CXC
    public class ApiResponse_PARAMETROS_CXC
    {
        public bool success { get; set; }
        public bool exito { get; set; }
        public st_formaspago[] formaspago { get; set; }
        public st_bancos[] bancos { get; set; }
        public st_tarjetas[] tarjetas { get; set; }
        public st_cuentas[] cuentas { get; set; }
        public st_modulosnc[] modulosnc { get; set; }
        public st_tiposnc[] tiposnc { get; set; }
        
    }
}
