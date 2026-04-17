using Newtonsoft.Json;
using SQLite;

namespace DMSA.Models.Odoo.Tareas
{
    [Table("account_analytic_line")]
    public class AccountAnalyticLine
    {
        [PrimaryKey]
        [AutoIncrement]
        public int id { get; set; }
        public string name { get; set; }
        //[Ignore]
        //[JsonIgnore]
        //public string? description { get; set; } 
        public int account_id { get; set; }
        public DateTime date { get; set; }
        public decimal amount { get; set; }
        public decimal unit_amount { get; set; }
        public int motivo { get; set; }
        public int? partner_id { get; set; }
        public int user_id { get; set; }
        public int company_id { get; set; }
        public int currency_id { get; set; }
        public int create_uid { get; set; }
        public int task_id { get; set; }
        public int parent_task_id { get; set; }
        public int project_id { get; set; }
        public int employee_id { get; set; }
        public int department_id { get; set; }
        public decimal hour_start { get; set; }
        public decimal hour_end { get; set; }
        public decimal duration { get; set; }
        public int x_account_id_1 { get; set; }
        [JsonIgnore]
        public bool is_synchronized { get; set; }
        [JsonIgnore]
        public DateTime date_synchronized { get; set; }
        [JsonIgnore]
        public int id_sync { get; set; }
        [JsonIgnore]
        public int task_id_sync { get; set; }

        [Ignore]
        [JsonIgnore]
        public string res_company_display { get; set; }
        [Ignore]
        [JsonIgnore]
        public string motivo_display { get; set; }
        [Ignore]
        [JsonIgnore]
        public string res_partner_display { get; set; }
    }
}
