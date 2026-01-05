using DMSA.Models.Odoo.DMCobranzas;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Accounting
{
    public class AccountTypeModule
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }

        [JsonIgnore]
        public int _module_id { get; set; }
        [Ignore]
        public AccountModule[] module_id { get; set; }

        [JsonProperty("type")]
        //[SQLite.Column("type_move")]
        public string type_move { get; set; }
        public bool consider_report { get; set; }
        public bool consider_commissions { get; set; }
        //public object[] product_id { get; set; }
        public bool state { get; set; }
        public bool use_point_sale { get; set; }
        public DateTime create_date { get; set; }
        public DateTime write_date { get; set; }
    }
}
