namespace DMSA.Models.Odoo.DMOrders.promotions.@abstract
{
    public sealed class PromotionEvalItem
    {
        public PromotionBenefit Promotion { get; set; } = new();
        public RuleInfo? Rule { get; set; }
        public double Discount { get; set; }
        public List<string> Reasons { get; set; } = new();
    }

    //public sealed class PromotionEvalItem
    //{
    //    public PromotionHeader Promotion { get; set; }
    //    public RuleInfo Rule { get; set; }
    //    public double Discount { get; set; }
    //    public List<string> Reasons { get; set; } = new();
    //}
}
