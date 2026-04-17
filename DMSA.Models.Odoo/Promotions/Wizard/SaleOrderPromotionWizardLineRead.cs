using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace DMSA.Models.Odoo.Promotions.Wizard
{
    [Table("SaleOrderPromotionWizardLineRead")]
    public class SaleOrderPromotionWizardLineRead : OdooEntity
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("wizard_id")]
        public JToken Wizard_Id { get; set; }

        [JsonProperty("promotion_id")]
        public JToken Promotion_Id { get; set; }

        [JsonProperty("rule_id")]
        public JToken Rule_Id { get; set; }

        [JsonProperty("promotions_type_id")]
        public JToken Promotions_Type_Id { get; set; }

        [JsonProperty("lines_ids")]
        public int[] Lines_Ids { get; set; }
    }
}
