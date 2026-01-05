using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

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
        public int _account_id
        {
            get => GetId(account_id);
            set => account_id = SetId(account_id, value);
        }

        [Ignore]
        public JToken move_id { get; set; }
        public int _move_id
        {
            get => GetId(move_id);
            set => move_id = SetId(move_id, value);
        }

        [Ignore]
        public JToken product_id { get; set; }
        public int _product_id
        {
            get => GetId(product_id);
            set => product_id = SetId(product_id, value);
        }

        [Ignore]
        public JToken analytic_line_ids { get; set; }
        
        [JsonIgnore]
        [Column("_analytic_line_ids")]
        public string _analytic_line_ids
        {
            get => SetIdsJson(analytic_line_ids);
            set { }
        }

        [JsonProperty("create_date")]
        [Column("create_date")]
        public DateTime? create_date { get; set; }

        
        [JsonProperty("write_date")]
        [Column("write_date")]
        public DateTime? write_date { get; set; }
    }
}
