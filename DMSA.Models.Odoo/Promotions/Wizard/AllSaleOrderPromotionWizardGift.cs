using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;

namespace DMSA.Models.Odoo.Promotions.Wizard
{
    public class AllSaleOrderPromotionWizardGift : OdooEntity
    {        
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("wizard_id")]
        public int Wizard_Id { get; set; }

        [JsonProperty("promotion_line_id")]
        public int Promotion_Line_Id { get; set; }

        [JsonProperty("product_id")]
        public int Product_Id { get; set; }

        [JsonProperty("currency_id")]
        public int Currency_Id { get; set; }


        [JsonProperty("lines_ids")]
        public int[] Lines_Ids { get; set; }


        [JsonProperty("default_code")]
        public string Default_Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("stock")]
        public decimal Stock { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("qty")]
        public decimal Qty { get; set; }

        [JsonProperty("approve")]
        public bool Approve { get; set; }

        [JsonProperty("discount")]
        public int Discount { get; set; }

        [JsonProperty("obtained")]
        public bool Obtained { get; set; }
    }

    public class AllSaleOrderPromotionWizardGiftWrapper : List<object>
    {
        public AllSaleOrderPromotionWizardGiftWrapper(AllSaleOrderPromotionWizardGift line)
        {
            Add(0);
            Add(0);
            Add(line);
        }
    }
}
