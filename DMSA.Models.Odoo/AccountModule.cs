using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Models
{
    public class AccountModule
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }
    }
}
