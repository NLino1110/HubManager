using DMSA.Models.Odoo.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMOrders.promotions.abstractCustom
{
    public class PromoRuleMatch
    {
        public int id { get; set; }
        public int promo_id { get; set; }
        public int promotion_type_id { get; set; }
        public int product_id { get; set; }
        public int product_uom_id { get; set; }
        public int selection_type_id { get; set; }
        public int payment_method_id { get; set; }
        public int raffle_template_id { get; set; }
        public int change_id { get; set; }
        public string general_grupor_tipo_id_json { get; set; }
        public string variable { get; set; }
        public string operator_ { get; set; }
        public int value { get; set; }
        public int minimum_value { get; set; }
        public int maximum_value { get; set; }
        public string product_promotion { get; set; }
        public string code { get; set; }
        public int qty { get; set; }
        public string is_fixed { get; set; }
        public int discount { get; set; }
        public int discount_base { get; set; }
        public int count_products { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public bool unlimited_time { get; set; }
        public bool state { get; set; }
        public string type { get; set; }




        public bool IsDiscount { get; set; }
        public double Discount { get; set; }
        public int ProductTmplId { get; set; }
        public int ProductId { get; set; }
        public int AllowedGifts { get; set; }
        public List<string> Reasons { get; set; } = new();
    }
}
