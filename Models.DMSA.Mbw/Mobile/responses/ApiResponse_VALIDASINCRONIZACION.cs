using CobranzasDMSA.Models;
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
    public class ApiResponse_VALIDASINCRONIZACION
    {
        public bool exito { get; set; }
        public bool habilitado { get; set; }        
        public DateTime fecha { get; set; }        
        public Empresa[]? empresas { get; set; }

        //public string? mensaje { get; set; }
    }
}
