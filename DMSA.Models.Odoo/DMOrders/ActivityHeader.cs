using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMOrders
{
    public class ActivityHeader
    {
        [PrimaryKey]
        public int id { get; set; }
        public int user_id { get; set; }
        public int seller_id { get; set; }
        public int status { get; set; }
        public DateTime planning_date { get; set; }
        public DateTime create_date { get; set; }
        public DateTime write_date { get; set; }

        [Ignore]
        public string user_name { get; set; }
        [Ignore]
        public string seller_name { get; set; }
    }
}
