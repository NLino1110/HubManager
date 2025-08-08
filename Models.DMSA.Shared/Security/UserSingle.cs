//using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DMSA.Shared.Security
{
    public class UserSingle
    {        
        public string codigo { get; set; }
        public string nombre { get; set; }
        public string clave { get; set; }
    }
}
