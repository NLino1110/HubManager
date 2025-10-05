using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Database.Sqlite
{
    public class ActivityHeaderDb
    {
        SQLiteAsyncConnection Database;

        public ActivityHeaderDb()
        {

        }

        public async Task<int> GetCount()
        {
            return (await Database.Table<ActivityHeader>().ToListAsync()).Count;
        }

        public async Task<List<ActivityHeader>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<ActivityHeader>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<ActivityHeader> GetItem(int id)
        {
            await Init();
            return await Database.Table<ActivityHeader>().Where(i => i.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(ActivityHeader item)
        {
            await Init();

            await Database.InsertOrReplaceAsync(item);

            return 0;
        }

        public async Task<int> InsertBatchAsync(ActivityHeader[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE");
            return 0;
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath);
            var result = await Database.CreateTableAsync<ActivityHeader>();
        }

        public async Task<int> Truncate()
        {
            await Init();            
            return await Database.DeleteAllAsync<ActivityHeader>();
        }
    }
}
