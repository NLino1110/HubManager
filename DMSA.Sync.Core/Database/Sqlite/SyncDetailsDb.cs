using DMSA.Models.Odoo;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class SyncDetailsDb
    {
        SQLiteAsyncConnection Database;

        public SyncDetailsDb()
        {

        }

        public async Task<int> GetCount()
        {
            await Init();
            return (await Database.Table<SyncDetails>().ToListAsync()).Count;
        }

        public async Task<SyncDetails> GetItemsAsync(int year, int month)
        {
            await Init();
            return await Database.Table<SyncDetails>().Where(x => x.year == year &&
            x.month == month).FirstOrDefaultAsync();
        }

        public async Task<List<SyncDetails>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<SyncDetails>().ToListAsync();
        }

        public async Task<SyncDetails> GetItem(string model_name)
        {
            await Init();
            return await Database.Table<SyncDetails>().Where(x => x.model_name == model_name).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(SyncDetails item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(SyncDetails[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE", true);
            return 0;
        }

        public async Task<int> Truncate()
        {
            await Init();

            return await Database.DeleteAllAsync<SyncDetails>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<SyncDetails>();
        }
    }
}
