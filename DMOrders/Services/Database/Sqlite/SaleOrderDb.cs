using DMSA.Models.Odoo.Native;
using SQLite;

namespace DMOrders.Services.Database.Sqlite
{
    public class SaleOrderDb : SqliteDbBase<sale_order>
    {       
        public SaleOrderDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }
                        
        public async Task<List<sale_order>> GetItemsAsync(int company_id)
        {
            await Init();
            return await Database.Table<sale_order>()
                .Where(x => x._company_id == company_id)
                .ToListAsync();
        }

        public async Task<List<sale_order>> GetItemsAsync(int company_id, bool sync_status)
        {
            await Init();
            return await Database.Table<sale_order>()
                .Where(x => x._company_id == company_id && x.is_synchronized == sync_status)
                .ToListAsync();
        }

        public async Task<List<sale_order>> GetItemsAsync(int partner_id, int company_id)
        {
            await Init();
            return await Database.Table<sale_order>()
                .Where(x => x._partner_id == partner_id && x._company_id == company_id)
                .ToListAsync();
        }

        public async Task<sale_order> GetItem(int id)
        {
            await Init();
            return await Database.Table<sale_order>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> DeleteRecursive(sale_order parent)
        {
            await Init();
            int count = 0;

            var resultItemsMove = (await Database.Table<sale_order>().ToListAsync()).Where(i => i.id == parent.id);

            foreach (var moveItem in resultItemsMove)
            {
                var resultItems = (await Database.Table<sale_order_line>().ToListAsync()).Where(i => i._order_id == moveItem.id);

                //Elimina detalles
                foreach (var item in resultItems)
                {
                    count++;
                    await Database.DeleteAsync(item);
                }

                //Eliminar movimientos
                await Database.DeleteAsync(moveItem);
            }

            //Elimina cabecera
            await Database.DeleteAsync(parent);
            return count;
        }

        private AsyncTableQuery<sale_order> BuildQuery(
            string filter_code,
            int filter_partner,
            DateTime? filter_datestart,
            DateTime? filter_dateend,
            int filter_status,
            int filter_sort)
        {
            Init();

            var q = Database.Table<sale_order>();

            // --- 1) Filtro por code (prioridad máxima, como tu método actual) ---
            if (!string.IsNullOrWhiteSpace(filter_code))
            {
                var raw = filter_code.Trim();

                // Si es numérico: buscar por id exacto (fallback lo haces fuera)
                if (int.TryParse(raw, out var idCode))
                {
                    q = q.Where(x => x.id == idCode);
                    // OJO: no aplicamos más filtros aquí para mantener tu comportamiento original.
                    return ApplySort(q, filter_sort);
                }                
            }

            // --- 2) Resto de filtros cuando NO hay filter_code ---
            if (filter_partner > 0)
            {                
                q = q.Where(x => x._partner_id == filter_partner);
            }

            if (filter_datestart != null && filter_dateend != null)
                q = q.Where(x => x.date_order >= filter_datestart && x.date_order <= filter_dateend);

            //if (filter_new == 1)
            //    q = q.Where(x => x.is_new);

            if (filter_status == 1)
                q = q.Where(x => x.is_synchronized);
            else if (filter_status == 2)
                q = q.Where(x => !x.is_synchronized);

            // --- 3) Orden ---
            q = ApplySort(q, filter_sort);

            return q;
        }

        private static AsyncTableQuery<sale_order> ApplySort(
            AsyncTableQuery<sale_order> q, int filter_sort)
        {
            // 1: sequence ASC, 2: code ASC, 3: name ASC; default: id ASC
            return filter_sort switch
            {
                0 => q.OrderByDescending(x => x.date_order),
                1 => q.OrderBy(x => x.id),
                2 => q.OrderBy(x => x._partner_id),
                3 => q.OrderBy(x => x.date_order),
                _ => q.OrderBy(x => x.is_synchronized)
            };
        }


        public async Task<(IList<sale_order> Items, int Total)> GetPagedAsync(
            string filter_code,
            int filter_partner,
            DateTime? filter_datestart,
            DateTime? filter_dateend,
            int filter_status,
            int filter_sort,
            int page, int pageSize, CancellationToken ct = default)
        {
            var q = BuildQuery(filter_code,
            filter_partner,
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
