using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Abstract
{
    public class OriginPromoOrderLine
    {
        public int sequence { get; set; }
        public int product_id { get; set; }
        public int product_tmpl_id { get; set; }
        public int rule_id { get; set; }
        public int promo_id { get; set; }
        public int total_allowed_gifts { get; set; }
    }
}
