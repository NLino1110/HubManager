using CobranzasDMSA_Odoo.Models;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.General.Responses
{
    [Obsolete("Debe eliminarse")]
    public class ApiResponseJournal: ApiResponseOdoo
    {        
        public account_journal[] data { get; set; }
    }
}
