using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models
{
    public class CobCierreDetalle
    {
        public int cantidad { get; set; }
        public decimal total { get; set; }
        public List<CobCierreDetalle_data> data { get; set; }
    }

    public class CobCierreDetalle_data
    {
        public string idformapago { get; set; }
        public decimal valor { get; set; }
        public string ref1 { get; set; }
        public string ref2 { get; set; }
        public string ref3 { get; set; }
    }
}
