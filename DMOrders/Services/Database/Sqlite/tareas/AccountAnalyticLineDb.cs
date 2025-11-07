using DMSA.Models.Odoo.DMOrders.tareas;
using SQLite;

namespace DMOrders.Services.Database.Sqlite
{
    public class AccountAnalyticLineDb : SqliteDbBase<AccountAnalyticLine>
    {
        public AccountAnalyticLineDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<AccountAnalyticLine>> GetItemsAsync(ProjectTask parent)
        {
            await Init();
            return await Database.Table<AccountAnalyticLine>().Where(x=> x.project_id == parent.project_id_ && x.task_id == parent.id).ToListAsync();
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
            return await Database.Table<AccountAnalyticLine>().Where(x=>x.name == name).ToListAsync();
        }

        public async Task<AccountAnalyticLine> GetItem(int id)
        {
            await Init();
            return await Database.Table<AccountAnalyticLine>().Where(x=>x.id == id).FirstOrDefaultAsync();
        } 
    }
}
