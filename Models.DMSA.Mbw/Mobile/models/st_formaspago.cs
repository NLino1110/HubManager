using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.MovilCobranzas.Api
{   
    public class st_formaspago
    {
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public string tipo { get; set; }
        public string porcentaje { get; set; }
    }
}
