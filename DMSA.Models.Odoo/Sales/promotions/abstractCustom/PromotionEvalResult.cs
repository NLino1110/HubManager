
namespace DMSA.Models.Odoo.DMOrders.promotions.abstractCustom
{
    public sealed class PromotionEvalResult
    {
        public DateTime NowUtc { get; set; }
        public List<PromotionEvalItem> Items { get; set; } = new();
        public PromotionEvalItem? Best => Items.OrderByDescending(i => i.IsBest).FirstOrDefault();
    }
}
