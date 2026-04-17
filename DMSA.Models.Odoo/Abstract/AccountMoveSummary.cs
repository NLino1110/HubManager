using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Abstract
{
    public class AccountMoveSummary
    {
        public string docnum_mask { get; set; }
        public DateTime invoice_date { get; set; }
        public decimal total_amount_reconciled { get; set; }
        public decimal total_amount_residual { get; set; }
    }
}
