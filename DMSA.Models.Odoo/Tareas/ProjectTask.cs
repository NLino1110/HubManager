using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;

namespace DMSA.Models.Odoo.Tareas
{
    [Table("project_task")]
    public class ProjectTask : OdooEntity
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
        public string sync_status { get; set; } = ProjectTaskSyncStatus.Pending;

        [JsonIgnore]
        public string sync_message { get; set; } = string.Empty;

        [JsonIgnore]
        public DateTime last_sync_attempt { get; set; }

        [JsonIgnore]
        public int sync_ok_count { get; set; }

        [JsonIgnore]
        public int sync_total_count { get; set; }

        // Propiedad que sí se serializa como "state"
        [JsonProperty("state")]
        public string? state { get; set; }

        // Propiedad para UI, no participa en JSON
        [Ignore]
        [JsonIgnore]
        public string? state_view =>
                (state, is_synchronized, sync_status) switch
                {
                    (_, _, ProjectTaskSyncStatus.Complete) => "COMPLETA",
                    (_, _, ProjectTaskSyncStatus.Partial) => sync_total_count > 0
                        ? $"PARCIAL ({sync_ok_count}/{sync_total_count})"
                        : "PARCIAL",
                    (_, _, ProjectTaskSyncStatus.Error) => "ERROR",
                    (_, _, ProjectTaskSyncStatus.Pending) => "PENDIENTE",
                    ("draft", true, _) => "SINCRONIZADO",
                    ("draft", false, _) => "ACTIVO",
                    ("sent", _, _) => "SINCRONIZADO",
                    ("done", _, _) => "TERMINADO",
                    ("cancel", _, _) => "CANCELADO",
                    _ => state
                };

        [Ignore]
        [JsonIgnore]
        public bool IsFullySynced =>
            string.Equals(sync_status, ProjectTaskSyncStatus.Complete, StringComparison.OrdinalIgnoreCase);

        [Ignore]
        [JsonIgnore]
        public bool HasPendingDetails =>
            string.Equals(sync_status, ProjectTaskSyncStatus.Partial, StringComparison.OrdinalIgnoreCase)
            || (sync_total_count > 0 && sync_ok_count < sync_total_count)
            || (sync_total_count > 0 && id_sync <= 0);
    }
}
