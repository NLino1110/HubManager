using DMSA.Models.Odoo.Tareas;
using DMSA.Sync.Core.Database.Sqlite.tareas;
using SQLite;
using System.Linq;

namespace DMSA.Sync.Core.Database.Sqlite.Sales
{
    public class ProjectTaskDb : SqliteDbBase<ProjectTask>
    {
        public ProjectTaskDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        protected override async Task OnAfterInit()
        {
            await EnsureColumnAsync("sync_status", "TEXT");
            await EnsureColumnAsync("sync_message", "TEXT");
            await EnsureColumnAsync("last_sync_attempt", "TEXT");
            await EnsureColumnAsync("sync_ok_count", "INTEGER NOT NULL DEFAULT 0");
            await EnsureColumnAsync("sync_total_count", "INTEGER NOT NULL DEFAULT 0");
            await RepairSyncStatusAsync();
        }

        private async Task EnsureColumnAsync(string columnName, string columnTypeSql)
        {
            var cols = await Database.QueryAsync<SqliteColumnInfo>(
                "PRAGMA table_info(project_task)");

            if (cols != null && cols.Any(c =>
                    string.Equals(c.name, columnName, StringComparison.OrdinalIgnoreCase)))
                return;

            await Database.ExecuteAsync(
                $"ALTER TABLE project_task ADD COLUMN {columnName} {columnTypeSql}");
        }

        private async Task RepairSyncStatusAsync()
        {
            var tasks = await Database.Table<ProjectTask>().ToListAsync();
            var lineDb = new AccountAnalyticLineDb(DatabaseFilename);

            foreach (var task in tasks)
            {
                var lines = await lineDb.GetItemsAsync(task);
                if (lines == null || lines.Count == 0)
                {
                    if (task.is_synchronized && task.id_sync <= 0)
                    {
                        task.is_synchronized = false;
                        task.sync_status = ProjectTaskSyncStatus.Pending;
                        task.sync_message = "Cabecera sin ID en el ERP.";
                        await Database.UpdateAsync(task);
                    }
                    else if (string.IsNullOrWhiteSpace(task.sync_status))
                    {
                        task.sync_status = task.is_synchronized
                            ? ProjectTaskSyncStatus.Complete
                            : ProjectTaskSyncStatus.Pending;
                        await Database.UpdateAsync(task);
                    }
                    continue;
                }

                int ok = ProjectTaskSyncValidation.CountEffectivelySyncedLines(lines, task.id_sync);
                int total = lines.Count;
                bool changed = false;
                bool brokenLinkage = ProjectTaskSyncValidation.HasBrokenLineLinkage(lines, task.id_sync);
                bool needsHeaderRelink = ProjectTaskSyncValidation.NeedsHeaderRelink(task, lines);

                if (task.is_synchronized && ok < total)
                {
                    task.is_synchronized = false;
                    task.sync_status = ProjectTaskSyncStatus.Partial;
                    task.sync_ok_count = ok;
                    task.sync_total_count = total;
                    task.sync_message = brokenLinkage || needsHeaderRelink
                        ? "Detectado envío incompleto o sin vínculo ERP. Use Reprocesar pendientes."
                        : "Detectado envío incompleto. Use Reprocesar pendientes.";
                    changed = true;
                }
                else if (ok == total && needsHeaderRelink)
                {
                    task.sync_status = ProjectTaskSyncStatus.Partial;
                    task.sync_ok_count = ok;
                    task.sync_total_count = total;
                    task.is_synchronized = false;
                    task.sync_message = "La cabecera no tiene ID en el ERP. Use Reprocesar pendientes.";
                    changed = true;
                }
                else if (ok == total)
                {
                    task.sync_status = ProjectTaskSyncStatus.Complete;
                    task.sync_ok_count = ok;
                    task.sync_total_count = total;
                    task.is_synchronized = true;
                    changed = true;
                }
                else if (ok > 0 && ok < total)
                {
                    task.sync_status = ProjectTaskSyncStatus.Partial;
                    task.sync_ok_count = ok;
                    task.sync_total_count = total;
                    task.is_synchronized = false;
                    if (string.IsNullOrWhiteSpace(task.sync_message))
                        task.sync_message = "Hay detalles pendientes por sincronizar.";
                    changed = true;
                }
                else if (string.IsNullOrWhiteSpace(task.sync_status))
                {
                    task.sync_status = task.is_synchronized
                        ? ProjectTaskSyncStatus.Complete
                        : ProjectTaskSyncStatus.Pending;
                    task.sync_ok_count = ok;
                    task.sync_total_count = total;
                    changed = true;
                }

                if (changed)
                    await Database.UpdateAsync(task);
            }
        }

        private sealed class SqliteColumnInfo
        {
            public int cid { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public int notnull { get; set; }
            public string dflt_value { get; set; }
            public int pk { get; set; }
        }

        public async Task<List<ProjectTask>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<ProjectTask>().ToListAsync();
        }

        public async Task<List<ProjectTask>> GetItemsPendingSyncAsync(int company_id)
        {
            await Init();
            return await Database.Table<ProjectTask>()
                .Where(x => x.company_id == company_id
                    && (x.sync_status == ProjectTaskSyncStatus.Partial
                        || x.sync_status == ProjectTaskSyncStatus.Pending
                        || x.sync_status == ProjectTaskSyncStatus.Error
                        || !x.is_synchronized
                        || (x.id_sync <= 0 && x.sync_total_count > 0)))
                .ToListAsync();
        }

        public async Task<List<ProjectTask>> GetItemsAsync(int company_id, bool sync_status)
        {
            await Init();
            return await Database.Table<ProjectTask>()
                .Where(x => x.company_id == company_id && x.is_synchronized == sync_status)
                .ToListAsync();
        }

        public async Task<List<ProjectTask>> GetItemsAsync(int company_id, int user_id, int int_status, DateTime? dateStart, DateTime? dateEnd)
        {
            await Init();
            var query = Database.Table<ProjectTask>().Where(x => x.company_id == company_id && x.user_id == user_id);
            //int_status == -1 TODOS
            //int_status == 0 NO SINCRONIZADOS
            //int_status == 1 SINCRONIZADOS
            if (int_status == 0)
            {
                // NO SINCRONIZADOS
                query = query.Where(x => !x.is_synchronized);
            }
            else if (int_status == 1)
            {
                // SINCRONIZADOS
                query = query.Where(x => x.is_synchronized);
            }

            if (dateStart.HasValue && dateEnd.HasValue)
            {
                query = query.Where(x => x.date_assign >= dateStart && x.date_assign <= dateEnd);
            }

            return await query.ToListAsync();
        }

        public async Task<List<ProjectTask>> GetItemByNameAsync(int company_id, string name, int user_id)
        {
            await Init();
            return await Database.Table<ProjectTask>().Where(x=>x.name == name && x.company_id == company_id && x.user_id == user_id).ToListAsync();
        }

        public async Task<ProjectTask> GetItem(int id)
        {
            await Init();
            return await Database.Table<ProjectTask>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        private async Task<AsyncTableQuery<ProjectTask>> BuildQuery(            
            DateTime? filter_datestart,
            DateTime? filter_dateend,
            int filter_status,
            int filter_sort,
            int user_id)
        {
            await Init();

            var q = Database.Table<ProjectTask>();

            //Se filtra por usuario
            q = q.Where(x => x.user_id == user_id);

            if (filter_datestart != null && filter_dateend != null)
                q = q.Where(x => x.date_assign >= filter_datestart && x.date_assign <= filter_dateend);

            if (filter_status == 1)
                q = q.Where(x => x.is_synchronized);
            else if (filter_status == 2)
                q = q.Where(x => !x.is_synchronized);

            // --- 3) Orden ---
            q = ApplySort(q, filter_sort);

            return q;
        }

        private static AsyncTableQuery<ProjectTask> ApplySort(
            AsyncTableQuery<ProjectTask> q, int filter_sort)
        {
            // 1: sequence ASC, 2: code ASC, 3: name ASC; default: id ASC
            return filter_sort switch
            {
                0 => q.OrderByDescending(x => x.date_assign),
                1 => q.OrderBy(x => x.id),                
                _ => q.OrderBy(x => x.is_synchronized)
            };
        }

        public async Task<(IList<ProjectTask> Items, int Total)> GetPagedAsync(            
            DateTime? filter_datestart,
            DateTime? filter_dateend,
            int filter_status,
            int filter_sort,
            int user_id,
            int page, 
            int pageSize, 
            CancellationToken ct = default)
        {
            var q = await BuildQuery(
            filter_datestart,
            filter_dateend,
            filter_status,
            filter_sort,
            user_id);

            // COUNT(*) en SQLite, sin traer datos
            var total = await q.CountAsync();

            // LIMIT/OFFSET en SQLite (Skip/Take sobre AsyncTableQuery)
            var offset = Math.Max(0, (page - 1) * pageSize);
            var items = await q.Skip(offset).Take(pageSize).ToListAsync();

            return (items, total);
        }
    }
}
