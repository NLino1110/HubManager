using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMOrders.Sales.Abstract
{
    [Table("promo_rules")]
    public class OrderPromoApplied : OdooEntity
    {
        [JsonProperty("order_id")]
        public int order_id { get; set; }
        [JsonProperty("promo_benefit_id")]
        public int promo_benefit_id { get; set; }
        [JsonProperty("reasons")]
        public string? reasons { get; set; }
        [JsonProperty("details")]
        public string? details { get; set; }
    }
}
