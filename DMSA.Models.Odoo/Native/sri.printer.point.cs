using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Native
{
    [Obsolete("Parece que esta clase ya no se utiliza")]
    public class sri_printer_point
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }
    }
}
