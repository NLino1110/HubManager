using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMOrders
{
    [Obsolete]
    public class ActivityDetailsData
    {
        [PrimaryKey]
        public int id { get; set; }
        public int user_id { get; set; }
        public int res_company { get; set; }
        public int res_partner { get; set; }
        public string? description { get; set; }
        public int status { get; set; }
        public int type_activity { get; set; }
        public DateTime start_date_time { get; set; }
        public DateTime finish_date_time { get; set; }
        public DateTime create_date { get; set; }
        public DateTime write_date { get; set; }        
    }
}
