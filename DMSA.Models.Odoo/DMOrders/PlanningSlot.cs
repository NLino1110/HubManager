using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMOrders
{  
    public class PlanningSlot
    {
        [PrimaryKey]
        public int id { get; set; }
        public string resource_type { get; set; }
        public string state { get; set; }
        public bool is_past { get; set; }
        public bool is_unassign_deadline_passed { get; set; }
        public string work_email { get; set; }
        public bool previous_template_id { get; set; }
        public bool template_reset { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        public string display_name { get; set; }

        [Ignore]
        public JToken employee_id { get; set; }
        public JToken resource_id { get; set; }
        public JToken work_address_id { get; set; }
        public JToken department_id { get; set; }
        public JToken user_id { get; set; }
        public JToken company_id { get; set; }        
        public JToken role_id { get; set; }
        public JToken recurrency_id { get; set; }
        public JToken template_id { get; set; }
        public TimeSpan start_datetime { get; set; }
        public TimeSpan end_datetime { get; set; }
        public float allocated_hours { get; set; }
        public float allocated_percentage { get; set; }


        public int overlap_slot_count { get; set; }
        public bool sale_order_id { get; set; }
        public bool is_assigned_to_me { get; set; }
        public bool allow_self_unassign { get; set; }        
        public object[] template_autocomplete_ids { get; set; }        
        public bool sale_line_id { get; set; }
        public bool sale_line_plannable { get; set; }         
        public bool repeat { get; set; }
        public int repeat_interval { get; set; }
        public string repeat_unit { get; set; }
        public string repeat_type { get; set; }
        public bool repeat_until { get; set; }
        public int repeat_number { get; set; }
        public bool allow_template_creation { get; set; }
        public bool template_creation { get; set; }

        [Ignore]
        [JsonIgnore]
        public string res_partner_display { get; set; }
        [Ignore]
        [JsonIgnore]
        public string res_company_display { get; set; }
    }
}
