using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.MovilCobranzas.Api
{
    public class st_cuentas
    {
        public int empresa { get; set; }
        public int codigo { get; set; }
        public int bank_id { get; set; }
        public string acc_number { get; set; }
        public string acc_holder_name { get; set; }
        public string descripcion { get; set; }
    }
}
