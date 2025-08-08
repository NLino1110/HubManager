using DMSA.Models.Clientes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.General.Requests
{
    [Obsolete("Debe ser eliminado.")]
    public class ApiRequestOdoo_v1
    {        
        public int uid { get; set; }
        public DateTime dateIni { get; set; }
        public DateTime dateEnd { get; set; }
        public int index { get; set; }
        public int limit { get; set; } = 300;
        public bool update { get; set; }

        //parametros extra
        public string? parameters { get; set; }

        //"macronegocios", 2, "a"
        public string? databasename { get; set; }
        public string? password { get; set; }
    }
}
