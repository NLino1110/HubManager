using DMSA.Models.Clientes;
using DMSA.Models.Odoo.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.General.Requests
{
    public class ApiRequestCheckCn
    {
        public credit_note_request_detail[] credit_note_lines { get; set; }
    }
}
