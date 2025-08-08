using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Native
{
    public class account_move_line_send
    {
        [AutoIncrement]
        [PrimaryKey]
        [JsonIgnore]
        public int line_id { get; set; }
        [JsonIgnore]
        public int parent_move_id { get; set; }
        [JsonIgnore]
        public int create_id { get; set; }
        public string name { get; set; }
        public int account_id { get; set; }
        public int move_id { get; set; }
        public int currency_id { get; set; }
        public decimal price_unit { get; set; }
        public decimal price_total { get; set; }
        public decimal quantity { get; set; }
        public int product_id { get; set; }
        public int sequence { get; set; }

        [JsonIgnore]
        //[Ignore]
        public decimal original_quantity { get; set; }

        [JsonIgnore]
        public decimal discount_percentage { get; set; }
        [JsonIgnore]
        public decimal discount_balance { get; set; }
        [JsonIgnore]
        public bool was_odoo_synced { get; set; }
    }
}
