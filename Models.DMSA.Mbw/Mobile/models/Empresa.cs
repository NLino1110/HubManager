using Models.DMSA.Mbw.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA.Models
{
    public class Empresa
    {
        public string empresa { get; set; }
        public string nombre { get; set; }

        public GenAgencias[] agencias { get; set; }
    }
}
