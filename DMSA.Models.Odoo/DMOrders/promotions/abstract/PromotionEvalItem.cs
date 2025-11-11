namespace DMSA.Models.Odoo.DMOrders.promotions.@abstract
{
    public sealed class PromotionEvalItem
    {
        public PromotionBenefit Promotion { get; set; } = new();        
        public PromoRules? RuleSet { get; set; }
        public double Discount { get; set; }
        public List<string> Reasons { get; set; } = new();
    }
}
