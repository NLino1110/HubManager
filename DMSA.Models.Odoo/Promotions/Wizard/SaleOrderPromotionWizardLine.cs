using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;

namespace DMSA.Models.Odoo.Promotions.Wizard
{
    public class SaleOrderPromotionWizardLine : OdooEntity
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("wizard_id")]
        public int[] Wizard_Id { get; set; }

        [JsonProperty("promotion_id")]
        public int[] Promotion_Id { get; set; }

        [JsonProperty("rule_id")]
        public int[] Rule_Id { get; set; }

        [JsonProperty("promotions_type_id")]
        public int[] Promotions_Type_Id { get; set; }

        [JsonProperty("lines_ids")]
        public int[] Lines_Ids { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("discount")]
        public int Discount { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("rule_value")]
        public int Rule_Value { get; set; }

        [JsonProperty("qty_confirmation")]
        public bool Qty_Confirmation { get; set; }

        [JsonProperty("error")]
        public bool Error { get; set; }

        /// <summary>
        /// new | processed | applied | error
        /// </summary>
        [JsonProperty("state")]
        public string State { get; set; }
    }
}
