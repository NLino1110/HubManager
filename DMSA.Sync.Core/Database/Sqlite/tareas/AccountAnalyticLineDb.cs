using DMSA.Models.Odoo.Tareas;
using System.Diagnostics;

namespace DMSA.Sync.Core.Database.Sqlite.tareas
{
    public class AccountAnalyticLineDb : SqliteDbBase<AccountAnalyticLine>
    {
        public AccountAnalyticLineDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<AccountAnalyticLine>> GetItemsAsync(ProjectTask parent)
        {
            await Init();
            return await Database.Table<AccountAnalyticLine>().Where(x => x.project_id == parent.project_id_ && x.task_id == parent.id).ToListAsync();
        }

        public async Task<List<AccountAnalyticLine>> GetItemsAsync(int company_id, bool sync_status)
        {
            await Init();
            return await Database.Table<AccountAnalyticLine>()
                .Where(x => x.company_id == company_id && x.is_synchronized == sync_status)
                .ToListAsync();
        }

        public async Task<List<AccountAnalyticLine>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<AccountAnalyticLine>().ToListAsync();
        }

        public async Task<List<AccountAnalyticLine>> GetItemByNameAsync(string name)
        {
            await Init();
            return await Database.Table<AccountAnalyticLine>().Where(x => x.name == name).ToListAsync();
        }

        public async Task<AccountAnalyticLine> GetItem(int id)
        {
            await Init();
            return await Database.Table<AccountAnalyticLine>().Where(x => x.id == id).FirstOrDefaultAsync();
        }

        // Nuevo: elimina por instancia y devuelve filas afectadas (con trazas)
        public async Task<int> DeleteAsync(AccountAnalyticLine item)
        {
            await Init();
            if (item == null)
            {
                Debug.WriteLine("[AccountAnalyticLineDb] DeleteAsync: item es null");
                return 0;
            }

            Debug.WriteLine($"[AccountAnalyticLineDb] DeleteAsync: intentando eliminar id={item.id}");
            int result = await Database.DeleteAsync(item);
            Debug.WriteLine($"[AccountAnalyticLineDb] DeleteAsync: filas afectadas = {result} for id={item.id}");
            return result;
        }

        // Nuevo: elimina por id (DELETE SQL directo) y devuelve filas afectadas
        public async Task<int> DeleteByIdAsync(int id)
        {
            await Init();
            Debug.WriteLine($"[AccountAnalyticLineDb] DeleteByIdAsync: ejecutando DELETE WHERE id={id}");
            // ExecuteAsync devuelve el número de filas afectadas
            int affected = await Database.ExecuteAsync("DELETE FROM account_analytic_line WHERE id = ?", id);
            Debug.WriteLine($"[AccountAnalyticLineDb] DeleteByIdAsync: filas afectadas = {affected} for id={id}");
            return affected;
        }
    }
}
