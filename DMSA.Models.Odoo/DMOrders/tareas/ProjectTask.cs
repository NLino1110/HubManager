using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMOrders.tareas
{
    [Table("project_task")]
    public class ProjectTask: OdooEntity
    {
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        public int id { get; set; }
        public string name { get; set; }        
        public string? description { get; set; }
        public int priority { get; set; }
        [Ignore]
        public JToken stage_id { get; set; }


        [JsonIgnore]
        public int stage_id_
        {
            get => GetId(stage_id);
            set => stage_id = SetId(stage_id, value);
        }

        [Ignore]
        public JToken project_id { get; set; }
        
        
        [JsonIgnore]
        public int project_id_
        {
            get => GetId(project_id);
            set => project_id = SetId(project_id, value);
        }

        public int company_id { get; set; }
        public int parent_id { get; set; }
        public int create_uid { get; set; }
        public bool display_in_project { get; set; }
        [Ignore]
        [JsonIgnore]
        public string create_user { get; set; }
        [Ignore]
        public int[] user_ids { get; set; }
        public int user_id { get; set; }
        [Ignore]
        [JsonIgnore]
        public string display_username { get; set; }
        public DateTime date_assign { get; set; }
        public DateTime date_deadline { get; set; }
        [JsonIgnore]
        public bool is_synchronized { get; set; }
        [JsonIgnore]
        public DateTime date_synchronized { get; set; }
        [JsonIgnore]
        public int id_sync { get; set; }

        [JsonIgnore]
        [JsonProperty("state")]
        public string? state { get; set; }

        [Ignore]
        [JsonIgnore]
        [JsonProperty("state")]
        public string? state_view =>
                (state, is_synchronized) switch
                {
                    ("draft", true) => "SINCRONIZADO",
                    ("draft", false) => "ACTIVO",
                    ("sent", _) => "SINCRONIZADO",                    
                    ("done", _) => "TERMINADO",
                    ("cancel", _) => "CANCELADO",
                    _ => state
                };
    }
}
