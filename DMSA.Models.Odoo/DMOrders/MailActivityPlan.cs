using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMOrders
{
    [Table("mail_activity_plan")]
    public class MailActivityPlan
    {        
        [PrimaryKey]
        public int id { get; set; }

        public int company_id { get; set; }
        
        public string name { get; set; }
        public string display_name { get; set; }

        public bool active { get; set; }

        [Ignore]
        public MailActivityPlanTemplate[] template_id { get; set; }

        public int user_id { get; set; }
    }
}
