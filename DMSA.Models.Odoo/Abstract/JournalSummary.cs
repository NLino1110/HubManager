using DMSA.Models.Odoo.DebitCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Abstract
{
    public class JournalSummary
    {
        public string Type { get; set; }
        public int TotalRecords { get; set; }
        public decimal TotalAmount { get; set; }
        public MultipleCobrosInvoiceLine[] Lines { get; set; }
    }
}
