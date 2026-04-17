using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Native;
using Newtonsoft.Json;
using SQLite;

namespace DMSA.Models.Odoo.Promotions.Wizard
{
    [Table("sale_order_promotion_wizard_gift")]
    public class SaleOrderPromotionWizardGift : OdooEntity
    {
        [JsonProperty("wizard_id")]
        public int wizard_id { get; set; }

        [JsonProperty("promotion_line_id")]
        public int promotion_line_id { get; set; }

        [JsonProperty("product_id")]
        public int product_id { get; set; }

        [JsonProperty("qty")]
        public decimal qty { get; set; }
        [JsonProperty("stock")]
        public decimal stock { get; set; }
        [JsonProperty("price")]
        public decimal price { get; set; }

    }

    public class SaleOrderPromotionWizardGiftWrapper : List<object>
    {
        public SaleOrderPromotionWizardGiftWrapper(SaleOrderPromotionWizardGift line)
        {
            Add(0);
            Add(0);
            Add(line);
        }
    }
}
