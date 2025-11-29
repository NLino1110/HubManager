using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMOrders.promotions.abstractCustom
{
    public sealed class PromotionEvalResultV2
    {
        public DateTime NowUtc { get; set; }
        public List<PromotionEvalItemV2> Items { get; set; } = new();
        public PromotionEvalItemV2? Best => Items.OrderByDescending(i => i.IsBest).FirstOrDefault();
    }
}
