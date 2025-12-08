using DMSA.Models.Odoo.DMOrders.tareas;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite.Sales
{
    public class ProjectTaskDb : SqliteDbBase<ProjectTask>
    {
        public ProjectTaskDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<ProjectTask>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<ProjectTask>().ToListAsync();
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

        public async Task<List<ProjectTask>> GetItemByNameAsync(int company_id, string name)
        {
            await Init();
            return await Database.Table<ProjectTask>().Where(x=>x.name == name && x.company_id == company_id).ToListAsync();
        }

        public async Task<ProjectTask> GetItem(int id)
        {
            await Init();
            return await Database.Table<ProjectTask>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        private AsyncTableQuery<ProjectTask> BuildQuery(            
            DateTime? filter_datestart,
            DateTime? filter_dateend,
            int filter_status,
            int filter_sort)
        {
            Init();

            var q = Database.Table<ProjectTask>();

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
            int page, 
            int pageSize, 
            CancellationToken ct = default)
        {
            var q = BuildQuery(
            filter_datestart,
            filter_dateend,
            filter_status,
            filter_sort);

            // COUNT(*) en SQLite, sin traer datos
            var total = await q.CountAsync();

            // LIMIT/OFFSET en SQLite (Skip/Take sobre AsyncTableQuery)
            var offset = Math.Max(0, (page - 1) * pageSize);
            var items = await q.Skip(offset).Take(pageSize).ToListAsync();

            return (items, total);
        }
    }
}
