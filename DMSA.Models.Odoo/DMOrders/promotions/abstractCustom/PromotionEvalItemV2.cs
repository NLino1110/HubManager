namespace DMSA.Models.Odoo.DMOrders.promotions.abstractCustom
{
    public sealed class PromotionEvalItemV2
    {
        public PromotionBenefit Promotion { get; set; } = new();
        public List<PromoRuleMatch> RuleSet { get; set; }
        public int TotalTimesAllowed { get; set; }
        public int FoundTimesApplies { get; set; }
        public int MaxAllowedGifts { get; set; }
        public int PricelistId { get; set; }
        public bool IsBest { get; set; }
        public int GiftsForRemove { get; set; }
    }
}
