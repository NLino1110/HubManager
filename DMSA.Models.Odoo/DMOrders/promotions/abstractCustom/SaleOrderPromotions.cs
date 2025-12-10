using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMOrders.promotions.abstractCustom
{
    [Table("sale_order_promotion")]
    public class SaleOrderPromotions
    {
        [PrimaryKey]
        [AutoIncrement]
        [Column("id")]
        [JsonProperty("id")]
        public int id { get; set; }
        [Column("order_id")]
        public int order_id { get; set; }
        [Column("promotion_id")]
        public int promotion_id { get; set; }

        [Column("promotion_type_id")]
        public int promotion_type_id { get; set; }

        [Column("promotion_selection_type_id")]
        public int promotion_selection_type_id { get; set; }

        [Column("promotion_centers")]
        public int promotion_centers { get; set; }
        
        [Column("times_inv")]
        public int times_inv { get; set; }
        
        [Column("times_inv_applied")]        
        public int times_inv_applied { get; set; }

        [Column("applied")]
        public bool applied { get; set; }

        [Column("max_gifts")]
        public int max_gifts { get; set; }

        [Column("assigned_gifts")]
        public int assigned_gifts { get; set; }

        [Column("related_product_tmpl_ids")]
        public string related_product_tmpl_ids { get; set; }
    }
}
