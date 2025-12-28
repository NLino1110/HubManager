using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Promotions.Wizard
{    
    public class SaleOrderPromotionWizard : OdooEntity
    {        
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("order_id")]
        public int[] Order_Id { get; set; }

        [JsonProperty("pricelist_id")]
        public int[] Pricelist_Id { get; set; }

        [JsonProperty("currency_id")]
        public int[] Currency_Id { get; set; }

        [JsonProperty("current_promotion_line_id")]
        public int[] Current_Promotion_Line_Id { get; set; }

        [JsonProperty("line_ids")]
        public int[] Line_Ids { get; set; }

        [JsonProperty("gift_line_ids")]
        public int[] Gift_Line_Ids { get; set; }

        [JsonProperty("all_gift_line_ids")]
        public int[] All_Gift_Line_Ids { get; set; }

        [JsonProperty("required_qty")]
        public decimal Required_Qty { get; set; }

        [JsonProperty("total_selected")]
        public decimal Total_Selected { get; set; }

        [JsonProperty("promo_type")]
        public string Promo_Type { get; set; }

        [JsonProperty("base")]
        public bool Base { get; set; }
    }
}
