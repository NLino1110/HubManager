using DMSA.Models.Clientes;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.General.Requests
{
    public class ApiRequestCheckCn
    {
        public account_move_line_send[] credit_note_lines { get; set; }
    }
}
