using DMSA.Models.Odoo.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace DMSA.Models.Odoo.Abstract
{
    public class SaleOrderDTO
    {
        [JsonProperty("sale_order")]
        public JObject SaleOrder { get; set; }
        [JsonProperty("promo_data")]
        public string PromoData { get; set; }
    }
}
