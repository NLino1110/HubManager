using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMOrders
{
    [Obsolete]
    public class MailActivityPlanTemplate
    {
        [Ignore]
        public MailActivityPlan plan_id { get; set; }
        public int activity_type_id { get; set; }
        public string summary { get; set; }
        public int responsible_type { get; set; }
        public int responsible_id { get; set; }

        public int res_partner_id { get; set; }
        public string time_start { get; set; }
        public string time_finish { get; set; }

    }
}
