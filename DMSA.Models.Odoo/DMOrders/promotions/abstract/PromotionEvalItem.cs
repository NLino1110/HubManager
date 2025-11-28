namespace DMSA.Models.Odoo.DMOrders.promotions.@abstract
{
    public sealed class PromotionEvalItem
    {
        public int? PromotionTypeId
        {
            get => Promotion?._promotion_type_id;
        }

        public PromotionBenefit Promotion { get; set; } = new();
        public PromoRules? RuleSet { get; set; }
        public int ProductTmplId { get; set; }
        public int ProductId { get; set; }
        public int AllowedGifts { get; set; }
        public int TotalTimesAllowed { get; set; }
        public int FoundTimesApplies { get; set; }
        public int MaxAllowedGifts { get; set; }
        public int PricelistId { get; set; }
        public double Discount { get; set; }
        public List<string> Reasons { get; set; } = new();        
    }
}
