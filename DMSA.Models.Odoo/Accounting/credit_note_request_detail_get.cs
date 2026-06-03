using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Accounting
{
    [Table("credit_note_request_detail_get")]
    public class credit_note_request_detail_get: OdooEntity
    {
        public int id { get; set; }
        
        public JToken line_id { get; set; }

        [JsonIgnore]
        public int line_id_
        {
            get => GetId(line_id);
            set => line_id = SetId(line_id, value);
        }

        [JsonIgnore]
        public int parent_id { get; set; }
        public int move_id { get; set; }
        public JToken product_id { get; set; }
        
        [JsonIgnore]
        public int product_id_
        {
            get => GetId(product_id);
            set => product_id = SetId(product_id, value);
        }
        [JsonIgnore]
        public decimal original_quantity { get; set; }
        public decimal quantity_invoiced { get; set; }
        public decimal quantity_available { get; set; }
        public decimal price_unit { get; set; }
        public decimal price_return { get; set; }
        public decimal quantity { get; set; }
        public decimal amount_tax { get; set; }
        public decimal price_subtotal { get; set; }



        [JsonIgnore]
        public int create_id { get; set; }
        [JsonIgnore]
        public string name { get; set; }
        [JsonIgnore]
        public int account_id { get; set; }
        [JsonIgnore]
        public int currency_id { get; set; }
        [JsonIgnore]
        public decimal price_total { get; set; }
        [JsonIgnore]
        public int sequence { get; set; }        

        [JsonIgnore]
        public decimal discount_percentage { get; set; }
        [JsonIgnore]
        public decimal discount_balance { get; set; }
        [JsonIgnore]
        public bool was_odoo_synced { get; set; }
    }
}
