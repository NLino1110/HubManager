using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Abstract
{
    public class PreloadParameters
    {        
        public required int[] TopMarcas { get; set; }
        public required int[] TopClientes { get; set; }
    }
}
