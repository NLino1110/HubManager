using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMOrders.promotions.@abstract
{
    public sealed class PromotionEvalResult
    {
        public DateTime NowUtc { get; set; }
        public List<PromotionEvalItem> Items { get; set; } = new();
        public PromotionEvalItem? Best => Items.OrderByDescending(i => i.Discount).FirstOrDefault();
    }
}
