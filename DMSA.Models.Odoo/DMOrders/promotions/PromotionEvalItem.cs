using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMOrders.promotions
{
    public sealed class PromotionEvalItem
    {
        public PromotionHeader Promotion { get; set; }
        public RuleInfo Rule { get; set; }
        public double Discount { get; set; }
        public List<string> Reasons { get; set; } = new();
    }
}
