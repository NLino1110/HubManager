using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Native
{
    public class account_move_line : OdooEntity
    {
        [PrimaryKey]
        public int id { get; set; }
        
        public int sequence { get; set; }
        public string name { get; set; }        
        public decimal quantity { get; set; }
        public decimal price_unit { get; set; }
        public decimal price_subtotal { get; set; }
        public decimal discount_balance { get; set; }
        public decimal price_total { get; set; }
        public decimal discount_percentage { get; set; }
        public string display_type { get; set; }

        public int moveId { get; set; }
        public int productId { get; set; }
        public int accountId { get; set; }

        //private int _moveId = 0;
        //public int moveId
        //{
        //    get
        //    {
        //        return get_from_token(move_id);
        //    }
        //    set
        //    {
        //        set_to_token(move_id, value);                
        //    }
        //}

        ////private int _productId = 0;
        //public int productId
        //{
        //    get
        //    {
        //        return get_from_token(product_id);
        //    }
        //    set
        //    {
        //        set_to_token(product_id, value);
        //    }
        //}

        ////private int _accountId = 0;
        //public int accountId
        //{
        //    get
        //    {                
        //        return get_from_token(account_id);
        //    }
        //    set
        //    {
        //        set_to_token(account_id, value);
        //    }
        //}

        //Agregados
        [Ignore]
        public JToken account_id { get; set; }
        [Ignore]
        public JToken move_id { get; set; }
        [Ignore]
        public JToken product_id { get; set; }
        [Ignore]
        public JToken analytic_line_ids { get; set; }
        
        
        [JsonProperty("create_date")]
        [Column("create_date")]
        public DateTime? create_date { get; set; }

        
        [JsonProperty("write_date")]
        [Column("write_date")]
        public DateTime? write_date { get; set; }
    }    
}
